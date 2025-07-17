using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class AutoGain
{
    // Constants
    // 마우스 정확도 향상 Off, 배율 1 기준 0~3000 counts/s 정도 크기
    const int binCount = 64; // how many bins are there
    const double binSize = 48f; // 속도 구간 크기(count / s)
    List<double> gainCurves = new List<double>(binCount);
    const double sensitivityInverseScaler = 100.0; // gain을 그대로 저장하면 자릿수가 너무 작아 100배 키워 저장. 사용시 1/100로 나눠서 사용.
    const double C = 0.0055;

    // Thresholds
    const double ANGULAR_THRESHOLD = Math.PI / 4.0;    // 45°
    const double OVERSHOOT_RATIO_THRESHOLD = 0.5;             // 50%
    const double INTERRUPT_RATIO_THRESHOLD = 0.5;             // 50%

    double subAimPoint = 0.95; // 목표 거리 대비 실제 이동 비율

    //Aim point estimation
    double processNoise = 0.2;
    double sensorNoise = 40.0;
    double estimatedError = 1.0;
    double kalmanGain = 1.0;
    double filteredAimPoint = 0.95;

    #region Log Fields
    // 로그 주기 설정
    private const int RecordInterval = 10;
    private int _updateCount = 0;

    // 기간별 누적 카운터
    private int _periodSubmovements = 0;
    private int _periodOvershoot = 0;
    private int _periodUndershoot = 0;

    // 로그 저장용 리스트
    private List<GainLogEntry> _gainLogs = new List<GainLogEntry>();

    // 로그 항목 구조체
    private struct GainLogEntry
    {
        public int UpdateCount;
        public double[] GainCurve;
        public double SubAimPoint;
        public int SubmovementCount;
        public int OvershootCount;
        public int UndershootCount;
    }
    #endregion

    public AutoGain(double initialGain)
    {
        for (int i = 0; i < binCount; i++)
        {
            // Gain Function 초기화: 초기 Gain 값으로 모든 bin에 동일한 Gain 적용
            gainCurves.Add(initialGain);
        }
    }


    #region Gain Calculation
    /// <summary>
    /// 마우스 입력에 Gain Function을 적용하여 카메라 회전 크기를 계산합니다.
    /// </summary>
    /// <param name="dx"> mouse delta x </param>
    /// <param name="dy"> mouse delta y </param>
    /// <param name="timespan"> time delta </param>
    /// <param name="dYaw"> 좌우 회전(y축 회전) delta </param>
    /// <param name="dPitch"> 상하 회전(x축 회전) delta </param>
    /// <returns></returns>
    public bool getTranslatedValue(double dx, double dy, double timespan, out double dYaw, out double dPitch)
    {

        double magnitude = Math.Sqrt(dx * dx + dy * dy);
        double speed = magnitude / (timespan / 1000);  // unit: count/sec

        // CDgain = ppi / cpi;
        double gain = getInterpolatedValue(speed / binSize, gainCurves);

        dYaw = dx * gain / sensitivityInverseScaler;
        dPitch = -dy * gain / sensitivityInverseScaler;

        return true;
    }

    public static double getInterpolatedValue(double index, List<double> list)
    {
        // 입력 인덱스가 소수점을 포함할 수 있으며 [0, list.Count - 1] 범위를 넘어갈 수 있음.
        // 범위를 넘어가면 최소/최댓값으로 Clamp
        // 나머지 경우는 소수점 이하 값 처리를 위해 선형 보간 실시

        int lowerIndex = (int)Math.Floor(index);
        int upperIndex = (int)Math.Ceiling(index);

        // minimum value (for out of index)
        if (lowerIndex < 0)
            return list[0];
        // maximum value (for out of index)
        if (upperIndex >= list.Count)
            return list[list.Count - 1];

        return linearMap(index, lowerIndex, upperIndex, list[lowerIndex], list[upperIndex]);
    }

    /// <summary>
    /// (x0, y0), (x1, y1) 사이의 선형 보간을 수행합니다.
    /// </summary>
    /// <param name="x"> 입력값 </param>
    /// <param name="x0"> 선형보간 시작점 x값 </param>
    /// <param name="x1"> 선형보간 끝점 x값 </param>
    /// <param name="y0"> 선형보간 시작점 y값 </param>
    /// <param name="y1"> 선형보간 끝점 y값 </param>
    /// <returns> 입력값이 x일 때의 y 값 </returns>
    public static double linearMap(double x, double x0, double x1, double y0, double y1)
    {
        if ((x1 - x0) == 0)
            return (y0 + y1) / 2;

        double ratio = (x - x0) / (x1 - x0);
        return (y1 - y0) * ratio + y0;
    }
    #endregion

    #region Gain Adjustment

    public void UpdateGainCurve(AGTrialData tdata)
    {
        // Gain Curve를 Trial Data에 따라 업데이트합니다.
        // tdata는 Trial의 속도, 움직임 등을 포함하는 데이터 구조체입니다.
        // 이 함수는 Trial Data를 분석하여 gainCurves를 조정합니다.

        // 1) 분석을 위해 Movement Profile 생성
        AGMovementData.Profiles profile = tdata.Movement.CreateSmoothedProfiles();
        if (profile.IsEmpty)
            return;


        // 2) 서브무브먼트 분할
        List<AGSubMovement> submovements = SegmentIntoSubmovements(profile);
        if (submovements.Count == 0) return;


        // 3) 포지션 및 타겟 좌표 획득
        List<TimePointR> positions = profile.Position;
        PointR pTarget = tdata.ThisTarget.posR;

        int normalCount = 0;

        // 4) 서브무브먼트 분석
        for (int i = 0; i < submovements.Count; i++)
        {
            // 구조체를 꺼내서 수정 후 다시 저장
            AGSubMovement sub = submovements[i];

            // 시작점/종료점
            TimePointR pStartTP = profile.Position[sub.MinStartIndex];
            TimePointR pEndTP = profile.Position[sub.MinEndIndex];

            // 2D 벡터로 변환
            Vector2 P_start = new Vector2((float)pStartTP.X, (float)pStartTP.Y);
            Vector2 P_end = new Vector2((float)pEndTP.X, (float)pEndTP.Y);
            Vector2 P_target = new Vector2((float)pTarget.X, (float)pTarget.Y);

            // 거리 계산
            double Dc = Vector2.Distance(P_start, P_end);

            Vector2 DcDirection = (P_end - P_start).normalized;
            Vector2 startToTarget = P_target - P_start;
            double Dtarget = Vector2.Dot(DcDirection, startToTarget);
            double overshootAmt = Math.Max(Dc - Dtarget, 0.0);

            // 최대 각 편차 계산
            Vector2 dirLine = startToTarget.normalized;
            double maxAngle = 0.0;
            for (int j = sub.MinStartIndex; j <= sub.MinEndIndex; j++)
            {
                TimePointR pt = positions[j];
                Vector2 P_j = new Vector2((float)pt.X, (float)pt.Y);
                Vector2 dirSeg = (P_j - P_start).normalized;
                if(dirSeg == Vector2.zero)
                {
                    Debug.LogWarning($"Zero vector encountered at index {j}. Skipping angle calculation.");
                    continue; // Skip zero vectors to avoid NaN in angle calculation
                }
                double dot = Vector2.Dot(dirLine, dirSeg);
                double angle = Math.Acos(Mathf.Clamp((float)dot, -1f, 1f));
                if (angle > maxAngle)
                {
                    maxAngle = angle;
                }
            }

            //Debug.Log($"maxAngle: {(float)maxAngle * Mathf.Rad2Deg:F3}, overshhotAmount: {overshootAmt:F2}, Dtarget:{Dtarget:F2}");

            // 분류 플래그 설정
            bool unaimed = (maxAngle > ANGULAR_THRESHOLD)
                         || (overshootAmt > OVERSHOOT_RATIO_THRESHOLD * Dtarget);
            bool interrupted = (!unaimed)
                            && (Dc < INTERRUPT_RATIO_THRESHOLD * Dtarget);

            sub.IsUnaimed = unaimed;
            sub.IsInterrupted = interrupted;

            if (!unaimed && !interrupted)
            {
                normalCount++;
                sub.IsNonBallistic = (normalCount > 2);
            }
            else
            {
                sub.IsNonBallistic = false;
            }
            
            sub.Dc = Dc;
            sub.Dtarget = Dtarget;
            double measured_P = (Dtarget != 0.0) ? Dc / Dtarget : 0.0;
            sub.measured_p = measured_P;

            // SpeedBins 계산
            sub.Si = new List<bool>(new bool[binCount]);

            for (int t = sub.MinStartIndex; t <= sub.MinEndIndex; t++)
            {
                double v = profile.RawVelocity[t].Y;           // pixel/s
                int bin = (int)(v / binSize);               // 0-based index
                if (bin < 0) bin = 0;
                if (bin >= binCount) bin = binCount - 1;

                // 3) 해당 bin 이 사용되었음을 표시
                sub.Si[bin] = true;
            }

            // 수정된 구조체를 리스트에 다시 할당
            submovements[i] = sub;
        }


        // 5) Gain Curve 업데이트 & AimPoint (p) 업데이트
        bool[] updatedBin = new bool[binCount];
        // 모든 submovements를 역순으로 순회하여, 
        // 동일한 빈에 두 번 이상 업데이트 되지 않도록 처리
        for (int i = submovements.Count - 1; i >= 0; i--)
        {
            // 구조체를 꺼내서 수정 후 다시 저장
            AGSubMovement sub = submovements[i];

            if (sub.IsUnaimed) continue;  // Unaimed: 건너뛰기

            // longitudinal error 계산
            bool isBallistic = !sub.IsNonBallistic && !sub.IsInterrupted;

            // Note: Aim-point 업데이트 시점
            if (!sub.IsUnaimed && !sub.IsInterrupted && !sub.IsNonBallistic)
            {
                updateAimPoint(sub.measured_p);
            }

            sub.Daim = (isBallistic || sub.IsInterrupted) ? subAimPoint * sub.Dtarget : sub.Dtarget;

            double R = sub.Daim - sub.Dc;

            if (R < 0) _periodOvershoot++;
            else if (R > 0) _periodUndershoot++;

            for (int j = 0; j < binCount; j++) // Note: 내부 for문 변수 오타 수정 완료
            {
                if (sub.Si[j] && !updatedBin[j])
                {
                    updatedBin[j] = true; // 해당 bin 업데이트 완료 표시
                    double gainDelta = C * R;
                    gainCurves[j] += gainDelta; // Gain Curve 업데이트
                    gainCurves[j] = Math.Max(gainCurves[j], 0.1); // Gain은 0.1보다 작아질 수 없음
                }
            }

            // 수정된 구조체를 리스트에 다시 할당
            submovements[i] = sub;
        }


        // 6) 로그 기록
        _periodSubmovements += submovements.Count;
        _updateCount++;
        // _periodOvershoot, _periodUndershoot는 Gain Curve 업데이트 시 이미 업데이트됨
        if (_updateCount % RecordInterval == 0)
        {
            GainLogEntry entry = new GainLogEntry
            {
                UpdateCount = _updateCount,
                GainCurve = gainCurves.ToArray(),
                SubAimPoint = subAimPoint,
                SubmovementCount = _periodSubmovements,
                OvershootCount = _periodOvershoot,
                UndershootCount = _periodUndershoot
            };
            _gainLogs.Add(entry);

            _periodSubmovements = 0;
            _periodOvershoot = 0;
            _periodUndershoot = 0;
        }

    }

    public List<AGSubMovement> SegmentIntoSubmovements(AGMovementData.Profiles profile)
    {
        int[] maxima = SeriesEx.Maxima(profile.RawVelocity, 0, -1);
        List<AGSubMovement> submovements = new List<AGSubMovement>(maxima.Length);
        if (maxima.Length == 0)
            return submovements;

        int[] minima = SeriesEx.Minima(profile.RawVelocity, 0, -1);
        List<(int index, bool isMax)> extrema = new List<(int, bool)>();
        foreach (var idx in minima) extrema.Add((idx, false));
        foreach (var idx in maxima) extrema.Add((idx, true));
        extrema.Sort((a, b) => a.index.CompareTo(b.index));

        if (extrema.Count >= 1 && extrema[0].isMax)
        {
            extrema.Insert(0, (0, false));
        }
        if (extrema.Count >= 1 && extrema[extrema.Count - 1].isMax)
        {
            extrema.Add((profile.RawVelocity.Count - 1, false));
        }

        string extremaStr = string.Join(", ", extrema.ConvertAll(e => $"({e.index}, {e.isMax})"));


        for (int i = 1; i < extrema.Count - 1; i++)
        {
            var first = extrema[i - 1 ];
            var second = extrema[i];
            var third = extrema[i + 1];

            if (!first.isMax && second.isMax && !third.isMax)
            {
                // 극소 - 극대 - 극소 패턴
                AGSubMovement submovement = new AGSubMovement
                {
                    MinStartIndex = first.index,
                    MaxIndex = second.index,
                    MinEndIndex = third.index
                };
                submovements.Add(submovement);
            }
        }

        return submovements;
    }

    public void updateAimPoint(double instant_aim_point)
    {
        estimatedError = estimatedError + sensorNoise;
        kalmanGain = processNoise / (processNoise + sensorNoise);
        filteredAimPoint = filteredAimPoint + kalmanGain * (instant_aim_point - filteredAimPoint);
        estimatedError = (1 - kalmanGain) * estimatedError;
        subAimPoint = filteredAimPoint;
    }
    #endregion

    public List<double> GetGainCurve()
    {
        // Gain Curve를 반환합니다.
        // Gain Curve는 현재 적용된 Gain 값들의 리스트입니다.
        return new List<double>(gainCurves);
    }

    public void ExportGainLogs(string filePath = "gain_log.csv")
    {
        if (_gainLogs.Count == 0)
            return;

        int binCount = _gainLogs[0].GainCurve.Length;
        var sb = new StringBuilder();

        // 1) 헤더: UpdateCount를 첫 열로
        var headers = new List<string>
        {
            "UpdateCount",
            "SubAimPoint",
            "OverShoot",
            "UnderShoot"
        };
        headers.AddRange(Enumerable.Range(0, binCount)
                                   .Select(i => $"bin{i}"));
        sb.AppendLine(string.Join(",", headers));

        // 2) 각 로그 엔트리
        foreach (var e in _gainLogs)
        {
            var fields = new List<string>
        {
            e.UpdateCount.ToString(),
            e.SubAimPoint.ToString("F4"),
            e.OvershootCount.ToString(),
            e.UndershootCount.ToString()
        };
            fields.AddRange(e.GainCurve.Select(g => g.ToString("F6")));
            sb.AppendLine(string.Join(",", fields));
        }

        // 3) 파일 쓰기
        File.WriteAllText(filePath, sb.ToString());
    }

}

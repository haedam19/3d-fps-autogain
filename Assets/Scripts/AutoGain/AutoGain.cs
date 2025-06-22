using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class AutoGain
{
    // Constants
    // ���콺 ��Ȯ�� ��� Off, ���� 1 ���� 0~4000 counts/s ���� ũ��
    const int binCount = 128; // how many bins are there
    const double binSize = 32f; // �ӵ� ���� ũ��(count / s)
    List<double> gainCurves = new List<double>(binCount);
    const double sensitivityInverseScaler = 100.0; // gain�� �״�� �����ϸ� �ڸ����� �ʹ� �۾� 100�� Ű�� ����. ���� 1/100�� ������ ���.
    const double C = 0.0005;

    // Thresholds
    const double ANGULAR_THRESHOLD = Math.PI / 4.0;    // 45��
    const double OVERSHOOT_RATIO_THRESHOLD = 0.5;             // 50%
    const double INTERRUPT_RATIO_THRESHOLD = 0.5;             // 50%

    double subAimPoint = 0.95; // ��ǥ �Ÿ� ��� ���� �̵� ����

    //Aim point estimation
    double processNoise = 0.2;
    double sensorNoise = 40.0;
    double estimatedError = 1.0;
    double kalmanGain = 1.0;
    double filteredAimPoint = 0.95;

    public AutoGain(double initialGain)
    {
        for (int i = 0; i < binCount; i++)
        {
            // Gain Function �ʱ�ȭ: �ʱ� Gain ������ ��� bin�� ������ Gain ����
            gainCurves.Add(initialGain);
        }
    }


    #region Gain Calculation
    /// <summary>
    /// ���콺 �Է¿� Gain Function�� �����Ͽ� ī�޶� ȸ�� ũ�⸦ ����մϴ�.
    /// </summary>
    /// <param name="dx"> mouse delta x </param>
    /// <param name="dy"> mouse delta y </param>
    /// <param name="timespan"> time delta </param>
    /// <param name="dYaw"> �¿� ȸ��(y�� ȸ��) delta </param>
    /// <param name="dPitch"> ���� ȸ��(x�� ȸ��) delta </param>
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
        // �Է� �ε����� �Ҽ����� ������ �� ������ [0, list.Count - 1] ������ �Ѿ �� ����.
        // ������ �Ѿ�� �ּ�/�ִ����� Clamp
        // ������ ���� �Ҽ��� ���� �� ó���� ���� ���� ���� �ǽ�

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
    /// (x0, y0), (x1, y1) ������ ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="x"> �Է°� </param>
    /// <param name="x0"> �������� ������ x�� </param>
    /// <param name="x1"> �������� ���� x�� </param>
    /// <param name="y0"> �������� ������ y�� </param>
    /// <param name="y1"> �������� ���� y�� </param>
    /// <returns> �Է°��� x�� ���� y �� </returns>
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
        // Gain Curve�� Trial Data�� ���� ������Ʈ�մϴ�.
        // tdata�� Trial�� �ӵ�, ������ ���� �����ϴ� ������ ����ü�Դϴ�.
        // �� �Լ��� Trial Data�� �м��Ͽ� gainCurves�� �����մϴ�.

        // 1) �м��� ���� Movement Profile ����
        AGMovementData.Profiles profile = tdata.Movement.CreateSmoothedProfiles();
        if (profile.IsEmpty)
            return;

        // 2) ���깫���Ʈ ����
        List<AGSubMovement> submovements = SegmentIntoSubmovements(profile);
        if (submovements.Count == 0) return;

        // 3) ������ �� Ÿ�� ��ǥ ȹ��
        List<TimePointR> positions = profile.Position;
        PointR pTarget = tdata.ThisTarget.posR;

        int normalCount = 0;

        // 4) ���깫���Ʈ �м�
        for (int i = 0; i < submovements.Count; i++)
        {
            // ����ü�� ������ ���� �� �ٽ� ����
            AGSubMovement sub = submovements[i];

            // ������/������
            TimePointR pStartTP = profile.Position[sub.MinStartIndex];
            TimePointR pEndTP = profile.Position[sub.MinEndIndex];

            // 2D ���ͷ� ��ȯ
            Vector2 P_start = new Vector2((float)pStartTP.X, (float)pStartTP.Y);
            Vector2 P_end = new Vector2((float)pEndTP.X, (float)pEndTP.Y);
            Vector2 P_target = new Vector2((float)pTarget.X, (float)pTarget.Y);

            // �Ÿ� ���
            double Dc = Vector2.Distance(P_start, P_end);

            Vector2 DcDirection = (P_end - P_start).normalized;
            Vector2 startToTarget = P_target - P_start;
            double Dtarget = Vector2.Dot(DcDirection, startToTarget);
            double overshootAmt = Math.Max(Dc - Dtarget, 0.0);

            // �ִ� �� ���� ���
            Vector2 dirLine = startToTarget.normalized;
            double maxAngle = 0.0;
            for (int j = sub.MinStartIndex; j <= sub.MinEndIndex; j++)
            {
                TimePointR pt = positions[j];
                Vector2 P_j = new Vector2((float)pt.X, (float)pt.Y);
                Vector2 dirSeg = (P_j - P_start).normalized;
                double dot = Vector2.Dot(dirLine, dirSeg);
                double angle = Math.Acos(Mathf.Clamp((float)dot, -1f, 1f));
                if (angle > maxAngle)
                {
                    maxAngle = angle;
                }
            }

            // �з� �÷��� ����
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

            // SpeedBins ���
            sub.Si = new List<bool>(new bool[binCount]);

            for (int t = sub.MinStartIndex; t <= sub.MinEndIndex; t++)
            {
                double v = profile.RawVelocity[t].Y;           // pixel/s
                int bin = (int)(v / binSize);               // 0-based index
                if (bin < 0) bin = 0;
                if (bin >= binCount) bin = binCount - 1;

                // 3) �ش� bin �� ���Ǿ����� ǥ��
                sub.Si[bin] = true;
            }

            // ������ ����ü�� ����Ʈ�� �ٽ� �Ҵ�
            submovements[i] = sub;
        }

        // 5) Gain Curve ������Ʈ & AimPoint (p) ������Ʈ
        bool[] updatedBin = new bool[binCount];
        // ��� submovements�� �������� ��ȸ�Ͽ�, 
        // ������ �� �� �� �̻� ������Ʈ ���� �ʵ��� ó��
        for (int i = submovements.Count - 1; i >= 0; i--)
        {
            // ����ü�� ������ ���� �� �ٽ� ����
            AGSubMovement sub = submovements[i];

            if (sub.IsUnaimed) continue;  // Unaimed: �ǳʶٱ�

            // longitudinal error ���
            bool isBallistic = !sub.IsNonBallistic && !sub.IsInterrupted;

            // Note: Aim-point ������Ʈ ����
            if (!sub.IsUnaimed && !sub.IsInterrupted && !sub.IsNonBallistic)
            {
                updateAimPoint(sub.measured_p);
            }

            sub.Daim = (isBallistic || sub.IsInterrupted) ? subAimPoint * sub.Dtarget : sub.Dtarget;

            double R = sub.Daim - sub.Dc;

            for (int j = 0; j < binCount; j++) // Note: ���� for�� ���� ��Ÿ ���� �Ϸ�
            {
                if (sub.Si[j] && !updatedBin[j])
                {
                    updatedBin[j] = true; // �ش� bin ������Ʈ �Ϸ� ǥ��
                    double gainDelta = C * R;
                    gainCurves[j] += gainDelta; // Gain Curve ������Ʈ
                }
            }


            

            // ������ ����ü�� ����Ʈ�� �ٽ� �Ҵ�
            submovements[i] = sub;
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
            extrema.Insert(0, (0, false));
        if (extrema.Count >= 1 && extrema[extrema.Count - 1].isMax)
            extrema.Add((profile.RawVelocity.Count - 1, false));

        for (int i = 1; i < extrema.Count - 1; i++)
        {
            var first = extrema[i];
            var second = extrema[i + 1];
            var third = extrema[i + 2];

            if (first.isMax && !second.isMax && third.isMax)
            {
                // �ؼ� - �ش� - �ؼ� ����
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
}

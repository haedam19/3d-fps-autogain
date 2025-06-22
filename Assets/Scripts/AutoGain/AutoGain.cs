using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class AutoGain
{
    // Constants
    // 마우스 정확도 향상 Off, 배율 1 기준 0~20000 counts/s 정도 크기
    const int binCount = 128; // how many bins are there
    const double binSize = 160f; // 속도 구간 크기(count / s)
    List<double> gainCurves = new List<double>(binCount);
    const double sensitivityInverseScaler = 100.0; // gain을 그대로 저장하면 자릿수가 너무 작아 100배 키워 저장. 사용시 1/100로 나눠서 사용.

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

        // 분석을 위해 Movement Profile 생성
        AGMovementData.Profiles profile = tdata.Movement.CreateSmoothedProfiles();
        if (profile.IsEmpty)
            return;

        List<AGSubMovement> submovements = SegmentIntoSubmovements(profile);
    }

    public List<AGSubMovement> SegmentIntoSubmovements(AGMovementData.Profiles profile)
    {
        List<AGSubMovement> submovements = new List<AGSubMovement>();

        int[] maxima = SeriesEx.Maxima(profile.Velocity, 0, -1);
        if (maxima.Length == 0)
            return submovements;

        int[] minima = SeriesEx.Minima(profile.Velocity, 0, -1);
        // create a submovement for each peak in the smoothed velocity profile
        for (int i = 0; i < maxima.Length; i++)
        {
            // dd
        }
        return submovements;
    }
    #endregion
}

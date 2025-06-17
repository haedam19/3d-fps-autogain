using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class AutoGain : MonoBehaviour
{
    // Constants
    const int binCount = 128; // how many bins are there
    const double binSize = 0.005; // 속도 구간 크기
    List<double> gainCurves = new List<double>(binCount);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

        dYaw = dx * gain;
        dPitch = -dy * gain;

        return true;
    }

    public static double getInterpolatedValue(double index, List<double> list)
    {
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
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class AutoGain : MonoBehaviour
{
    // Constants
    const int binCount = 128; // how many bins are there
    const double binSize = 0.005; // �ӵ� ���� ũ��
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
}

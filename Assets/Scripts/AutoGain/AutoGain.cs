using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class AutoGain
{
    // Constants
    // ���콺 ��Ȯ�� ��� Off, ���� 1 ���� 0~20000 counts/s ���� ũ��
    const int binCount = 128; // how many bins are there
    const double binSize = 160f; // �ӵ� ���� ũ��(count / s)
    List<double> gainCurves = new List<double>(binCount);
    const double sensitivityInverseScaler = 100.0; // gain�� �״�� �����ϸ� �ڸ����� �ʹ� �۾� 100�� Ű�� ����. ���� 1/100�� ������ ���.

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

        // �м��� ���� Movement Profile ����
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

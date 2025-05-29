using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// <para> ���ػ� Ÿ�̸��Դϴ�. </para>
/// <para> Time ������Ƽ�� ���� ���� ȣ�� �Ǵ� �ֱ� Reset �� ����� �ð��� ���� �� �ֽ��ϴ�.</para>
/// </summary>
public class Timer
{
    #region Import High Resolution Timer Function
    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(out long lpFrequency);
    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
    #endregion

    private static Timer _instance;
    public static Timer Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Timer();
            return _instance;
        }
    }

    private long _freq; // �ʴ� count Ƚ��
    private long _counter; // Time �����ϴ� ������ counter ��
    private long _startCounter; // Timer ���� ������ counter ��

    private Timer()
    {
        // instance ������ �ڵ� ���� 1ȸ ����
        QueryPerformanceFrequency(out _freq);
        QueryPerformanceCounter(out _startCounter);
    }

    /// <summary> Ÿ�̸Ӹ� �����մϴ�. </summary>
    public static void Reset()
    {
        QueryPerformanceCounter(out Instance._startCounter);
    }

    /// <summary> Ÿ�̸Ӱ� ���µ� ���� ����� �ð��� ms ������ ��ȯ�մϴ�. </summary>
    public static long Time
    {
        get
        {
            QueryPerformanceCounter(out Instance._counter);
            long time = (long)((double)(Instance._counter - Instance._startCounter) / ((double)Instance._freq / 1000));
            Debug.Log(time);
            return time;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// <para> 고해상도 타이머입니다. </para>
/// <para> Time 프로퍼티를 통해 최초 호출 또는 최근 Reset 후 경과된 시간을 얻을 수 있습니다.</para>
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

    private long _freq; // 초당 count 횟수
    private long _counter; // Time 측정하는 순간의 counter 값
    private long _startCounter; // Timer 리셋 순간의 counter 값

    private Timer()
    {
        // instance 생성시 자동 리셋 1회 시행
        QueryPerformanceFrequency(out _freq);
        QueryPerformanceCounter(out _startCounter);
    }

    /// <summary> 타이머를 리셋합니다. </summary>
    public static void Reset()
    {
        QueryPerformanceCounter(out Instance._startCounter);
    }

    /// <summary> 타이머가 리셋된 이후 경과된 시간을 ms 단위로 반환합니다. </summary>
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

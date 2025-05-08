using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 고해상도 타이머입니다. GameManager에서 Reset 후 사용해야 합니다.
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
    private long _startCounter; // Timer 초기화되는 순간의 counter 값

    private Timer()
    {
        QueryPerformanceFrequency(out _freq);
        QueryPerformanceCounter(out _startCounter);
    }

    /// <summary>
    /// 프로그램 실행 경과 시간을 ms 단위로 반환합니다.
    /// </summary>
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

    public static void Reset()
    {
        QueryPerformanceFrequency(out Instance._freq);
        QueryPerformanceCounter(out Instance._startCounter);
    }
}

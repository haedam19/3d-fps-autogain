using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AGSubMovement
{
    public int MinStartIndex;  // ���� �ؼ�
    public int MaxIndex;       // �ش�
    public int MinEndIndex;    // �� �ؼ�

    // classification flags
    public bool IsUnaimed;
    public bool IsInterrupted;
    public bool IsNonBallistic;

    // measurement values
    public double Dc;
    public double Dtarget;
    public double measured_p;

    // Speed bin
    public List<bool> speedbin;
}

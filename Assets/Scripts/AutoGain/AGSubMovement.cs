using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AGSubMovement
{
    public int MinStartIndex;  // 矫累 必家
    public int MaxIndex;       // 必措
    public int MinEndIndex;    // 场 必家

    // classification flags
    public bool IsUnaimed;
    public bool IsInterrupted;
    public bool IsNonBallistic;

    // measurement values
    public double Dc;
    public double Daim;
    public double Dtarget;
    public double measured_p;

    // Speed bin
    public List<bool> Si;

    public string ToString()
    {
        return $"IsUnaimed: {IsUnaimed}, IsInterrupted: {IsInterrupted}, IsNonBallistic: {IsNonBallistic}";

        //return $"MinStartIndex: {MinStartIndex}, MaxIndex: {MaxIndex}, MinEndIndex: {MinEndIndex}, " +
        //       $"IsUnaimed: {IsUnaimed}, IsInterrupted: {IsInterrupted}, IsNonBallistic: {IsNonBallistic}, " +
        //       $"Dc: {Dc}, Daim: {Daim}, Dtarget: {Dtarget}, measured_p: {measured_p}";
    }
}

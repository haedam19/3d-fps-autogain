using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MouseEvent
{
    public ushort conditionIndex;
    public ushort trialIndex;

    public ushort buttonFlag;
    public float x;
    public float y;
    public float time;

    public MouseEvent(ushort conditionIndex, ushort trialIndex, ushort buttonFlag, float x, float y, float time)
    {
        this.conditionIndex = conditionIndex;
        this.trialIndex = trialIndex;
        this.buttonFlag = buttonFlag;
        this.x = x;
        this.y = y;
        this.time = time;
    }
}

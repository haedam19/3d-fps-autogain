using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEvent
{
    public ushort buttonflags;
    public float x;
    public float y;
    public float time;

    public MouseEvent(ushort buttonflags, float x, float y, float time)
    {
        this.buttonflags = buttonflags;
        this.x = x;
        this.y = y;
        this.time = time;
    }
}

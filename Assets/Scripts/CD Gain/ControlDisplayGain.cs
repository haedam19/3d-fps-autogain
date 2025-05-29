using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ControlDisplayGain
{
    public enum Type{ Univariate, Bivariate, Angular }
    public Type type;

    private float _sensitivityX;
    private float _sensitivityY;

    public ControlDisplayGain(Type type, float sensitivity)
    {
        this.type = type;
        this._sensitivityX = sensitivity;
    }

    public Vector2 GainedDelta(Vector2 delta)
    {
        Vector2 gDelta;

        switch(type)
        {
            case Type.Univariate:
                gDelta = _sensitivityX * delta;
                break;
            case Type.Bivariate:
                gDelta = new Vector2(_sensitivityX * delta.x, _sensitivityY * delta.y);
                break;
            default:
                gDelta = _sensitivityX * delta;
                break;
        }
        return gDelta;
    }

}

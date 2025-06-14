using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AGTargetData
{
    public Vector3 posWorld;
    public Vector2 posScreen;
    public PointR posR;
    public float radius;
    public float w;
    
    public static AGTargetData Empty = new AGTargetData {
                                                posWorld = Vector3.zero,
                                                posScreen = Vector2.zero,
                                                posR = PointR.Empty,
                                                radius = 0f,
                                                w = 0f
                                            };

    public bool Contains(PointR p)
    {
        return PointR.Distance(posR, p) <= radius;
    }
}

public class AGTarget : MonoBehaviour
{
    public AGTargetData data;

    #region Properties (data 안 쓰고 바로 참조)
    public Vector3 posWorld
    {
        get => data.posWorld;
        set => data.posWorld = value;
    }
    public Vector2 posScreen
    {
        get => data.posScreen;
        set => data.posScreen = value;
    }
    public PointR posR
    {
        get => data.posR;
        set => data.posR = value;
    }
    public float radius
    {
        get => data.radius;
        set => data.radius = value;
    }
    public float w
    {
        get => data.w;
        set => data.w = value;
    }
    #endregion

    public bool Contains(PointR p)
    {
        return PointR.Distance(data.posR, p) <= data.radius;
    }

    public void RecordTargetData(float diameterInPixel)
    {
        posWorld = transform.position;
        posScreen = Camera.main.WorldToScreenPoint(posWorld);
        posR = (PointR)posScreen;
        w = diameterInPixel;
        radius = diameterInPixel / 2f;
    }
}

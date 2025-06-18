using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct AGTargetData
{
    public Vector3 posWorld;
    public Vector2 posRefScreen;
    public PointR posR;
    public double radius;
    public float w;
    
    public static AGTargetData Empty = new AGTargetData {
                                                posWorld = Vector3.zero,
                                                posRefScreen = Vector2.zero,
                                                posR = PointR.Empty,
                                                radius = 0f,
                                                w = 0f
                                            };

    public bool Contains(PointR p)
    {
        return PointR.Distance(posR, p) <= radius;
    }

    public bool IsEmpty()
    {
        return posWorld == Vector3.zero && posRefScreen == Vector2.zero && posR == PointR.Empty && radius == 0f && w == 0f;
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
    public Vector2 posRefScreen
    {
        get => data.posRefScreen;
        set => data.posRefScreen = value;
    }
    public PointR posR
    {
        get => data.posR;
        set => data.posR = value;
    }
    public double radius
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

        float _d = Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
        Vector3 cameraPos = Vector3.zero; // Camera.main.transform.position;
        Vector3 dir = (posWorld - cameraPos).normalized;

        float t = (_d - cameraPos.z) / dir.z;
        Vector3 intersection = cameraPos + dir * t;

        posRefScreen = (Vector2)intersection + new Vector2(Screen.width / 2f, Screen.height / 2f);

        posR = (PointR)posRefScreen;
        w = diameterInPixel;
        radius = diameterInPixel / 2f;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGCurveViewer : MonoBehaviour
{
    public List<double> curves;

    public void UpdateCurveView()
    {
        curves = AGManager.AG.GetGainCurve();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Xml;

public class TargetManager3D : MonoBehaviour
{
    [SerializeField] GameObject targetPrefab;
    [SerializeField] GameObject startingTargetPrefab;
    [SerializeField] GameObject targetRoot;

    int m_currentTrial;
    int m_currentTarget;
    int m_A;
    int m_W;
    List<GameObject> targetInstances;
    List<int> targetIndexSequence;

    public void Init()
    {
        targetInstances = new List<GameObject>();
        float distanceToCamera = 0.1f * Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
        transform.position = new Vector3(0f, 0f, distanceToCamera);
    }

    public void SetTargets(int targetCount, ConditionConfig condition)
    {
        m_A = condition.A;
        m_W = condition.W;

        // Reset
        if (targetInstances == null)
            targetInstances = new List<GameObject>();
        else
        {
            while (targetInstances.Count > 0)
            {
                GameObject target = targetInstances[0];
                targetInstances.RemoveAt(0);
                Destroy(target);
            }
        }

        for (int i = 0; i < targetCount; i++)
        {
            GameObject targetObj = Instantiate(targetPrefab, transform);
            float rad = (2 * Mathf.PI / targetCount) * i + Mathf.PI / 2f;
            float x = (m_A / 2) * Mathf.Cos(rad);
            float y = (m_A / 2) * Mathf.Sin(rad);
            targetObj.transform.localPosition = new Vector3(x, y, 0f);
            Target3D t = targetObj.GetComponent<Target3D>();
            t.Radius = m_W;
            t.posOnScreen = new Vector2(x, y);

            // Assume Camera Pos is (0, 0, 0). Adjust distance from camera to target to 0.1H / (2tan(FOV / 2)).
            targetObj.transform.position = targetObj.transform.position.normalized * transform.position.z;
            
            targetInstances.Add(targetObj);
        }

        int n = targetCount;
        bool even = n % 2 == 0;
        int k = even ? n / 2 : (n + 1) / 2;
        for (int i = 0; i < n; i++)
        {
            if(i / 2 == 0)
            {

            }
            else
            {

            }
        }

        m_currentTarget = 0;
        GameManager3D.Instance.trialIndex = 0;
        targetInstances[0].GetComponent<Target3D>().TargetOn();
    }

}

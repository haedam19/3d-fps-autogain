using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager3D : MonoBehaviour
{
    [SerializeField] GameObject targetPrefab;
    [SerializeField] int[] Aset;
    [SerializeField] int[] Wset;
    [SerializeField] int trialPerCondition;
    [SerializeField] int practice;
    [SerializeField] int blocks;

    int m_targetCount;
    int m_A; // Movement distance for each trial 
    int m_W; // Target width
    List<GameObject> targetInstances;

    private void Awake()
    {
        targetInstances = new List<GameObject>();
        float distanceToCamera = Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
        transform.position = new Vector3(0f, 0f, distanceToCamera);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTargets(int targetCount, int A, int W)
    {
        m_targetCount = targetCount;
        m_A = A;
        m_W = W;

        if (targetInstances == null)
            targetInstances = new List<GameObject>();

        targetInstances.Clear();
        for (int i = 0; i < targetCount; i++)
        {
            GameObject target = Instantiate(targetPrefab, transform);
            float rad = (2 * Mathf.PI / targetCount) * i;
            float x = (A / 2) * Mathf.Cos(rad);
            float y = (A / 2) * Mathf.Sin(rad);
            target.transform.localPosition = new Vector3(x, y, 0f);
        }
    }
}

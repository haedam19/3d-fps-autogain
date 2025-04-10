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

    int currentTarget;
    int m_A; // Movement distance for each trial 
    int m_W; // Target width
    List<GameObject> targetInstances;

    private void Awake()
    {
        targetInstances = new List<GameObject>();
        float distanceToCamera = Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
        transform.position = new Vector3(0f, 0f, distanceToCamera);
        SetTargets(trialPerCondition, Aset[0], Wset[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTargets(int targetCount, int A, int W)
    {
        m_A = A;
        m_W = W;

        if (targetInstances == null)
            targetInstances = new List<GameObject>();

        targetInstances.Clear();
        for (int i = 0; i < targetCount; i++)
        {
            GameObject targetObj = Instantiate(targetPrefab, transform);
            float rad = (2 * Mathf.PI / targetCount) * i;
            float x = (A / 2) * Mathf.Cos(rad);
            float y = (A / 2) * Mathf.Sin(rad);
            targetObj.transform.localPosition = new Vector3(x, y, 0f);
            Target t = targetObj.GetComponent<Target>();
            t.Radius = W;
            t.posOnScreen = new Vector2(x, y);

            // Assume Camera Pos is (0, 0, 0). Adjust distance from camera to target to H / (2 * tan(FOV / 2)).
            targetObj.transform.position = targetObj.transform.position.normalized * transform.position.z;
            
            targetInstances.Add(targetObj);
        }
    }
}

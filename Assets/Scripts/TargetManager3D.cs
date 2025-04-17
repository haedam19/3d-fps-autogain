using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetManager3D : MonoBehaviour
{
    Mouse mouse;

    [SerializeField] GameObject targetPrefab;
    [SerializeField] int[] Aset;
    [SerializeField] int[] Wset;
    [SerializeField] int trialPerCondition;
    [SerializeField] int practice;
    [SerializeField] int blocks;

    int m_currentTarget;
    int m_A; // Movement distance for each trial 
    int m_W; // Target width
    List<GameObject> targetInstances;

    private void Awake()
    {
        mouse = Mouse.current;


        targetInstances = new List<GameObject>();
        float distanceToCamera = 0.1f * Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
        transform.position = new Vector3(0f, 0f, distanceToCamera);
        SetTargets(trialPerCondition, Aset[0], Wset[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetTargets(int targetCount, int AIndex, int WIndex)
    {
        if (AIndex < 0 || WIndex < 0 || AIndex >= Aset.Length || WIndex >= Wset.Length)
        {
            Debug.LogError("Invalid Target Setting: Out of Index Error");
            return;
        }

        m_A = Aset[AIndex];
        m_W = Wset[WIndex];

        if (targetInstances == null)
            targetInstances = new List<GameObject>();
        else
            targetInstances.Clear();

        for (int i = 0; i < targetCount; i++)
        {
            GameObject targetObj = Instantiate(targetPrefab, transform);
            float rad = (2 * Mathf.PI / targetCount) * i;
            float x = (m_A / 2) * Mathf.Cos(rad);
            float y = (m_A / 2) * Mathf.Sin(rad);
            targetObj.transform.localPosition = new Vector3(x, y, 0f);
            Target t = targetObj.GetComponent<Target>();
            t.Radius = m_W;
            t.posOnScreen = new Vector2(x, y);

            // Assume Camera Pos is (0, 0, 0). Adjust distance from camera to target to 0.1H / (2tan(FOV / 2)).
            targetObj.transform.position = targetObj.transform.position.normalized * transform.position.z;
            
            targetInstances.Add(targetObj);
        }
    }
}

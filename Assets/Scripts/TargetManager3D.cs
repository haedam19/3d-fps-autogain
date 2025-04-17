using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct Condition
{
    public int A; // Diameter of the circle which targets are aligned along
    public int W; // Target width

    public Condition(int a, int w) { A = a; W = w; }
}

public class TargetManager3D : MonoBehaviour
{
    [SerializeField] GameObject targetPrefab;
    [SerializeField] int[] Aset;
    [SerializeField] int[] Wset;
    [SerializeField] int trialPerCondition;
    [SerializeField] int practice; // number of early trials considered to be practices
    [SerializeField] int blocks; // iterations (1 block = set of all conditions)

    int m_currentTrial;
    int m_currentTarget;
    int m_A;
    int m_W;
    List<GameObject> targetInstances;

    private void Awake()
    {
        targetInstances = new List<GameObject>();
        float distanceToCamera = 0.1f * Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
        transform.position = new Vector3(0f, 0f, distanceToCamera);
    }

    public void Initialize()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Condition> CreateConditionSequence()
    {
        List<Condition> conditionList = new List<Condition>();
        foreach (int A in Aset)
        {
            foreach (int W in Wset)
                conditionList.Add(new Condition(A, W));
        }

        // Shuffle COndition Sequence
        Condition temp;
        int length = conditionList.Count;
        int i, j;
        for (i = 0; i < length; i++)
        {
            j = UnityEngine.Random.Range(i, length);
            temp = conditionList[i];
            conditionList[i] = conditionList[j];
            conditionList[j] = temp;
        }

        return conditionList;
    }

    public void SetTargets(int targetCount, Condition condition)
    {
        m_A = condition.A;
        m_W = condition.W;

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
            float rad = (2 * Mathf.PI / targetCount) * i;
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
    }

}

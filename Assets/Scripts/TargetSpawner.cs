using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] GameObject target;
    GameObject currentTarget;

    [Header("Config")]
    [SerializeField] float margin;

    float xMin, xMax ,yMin, yMax;

    Vector2 center;

    // Start is called before the first frame update
    void Start()
    {
        xMin = margin;
        xMax = Screen.width - margin;
        yMin = margin;
        yMax = Screen.height - margin;
    }

    public Vector2 Spawn()
    {
        Destroy(currentTarget);

        Vector2 nextCenter;
        while (true)
        {
            float x = Random.Range(xMin, xMax);
            float y = Random.Range(yMin, yMax);
            nextCenter = new Vector2(x, y);

            if((center - nextCenter).sqrMagnitude > 10000f)
            {
                center = nextCenter;
                break;
            }
        }
        Vector2 nextCenterWorld = Camera.main.ScreenToWorldPoint(nextCenter);

        currentTarget = Instantiate(target, (Vector3)nextCenterWorld, Quaternion.identity);

        return center;
    }
}

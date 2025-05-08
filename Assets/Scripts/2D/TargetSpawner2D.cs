using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner2D : MonoBehaviour
{
    [SerializeField] GameObject target;
    [HideInInspector] public GameObject currentTarget;

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
        if(currentTarget)
            Destroy(currentTarget);

        float rad;
        if (GameManager2D.Instance.trial % 2 == 0)
            rad = (2 * Mathf.PI / GameManager2D.Instance.goal) * (GameManager2D.Instance.trial / 2f);
        else
            rad = (2 * Mathf.PI / GameManager2D.Instance.goal) * ((GameManager2D.Instance.trial - 1) / 2f) + Mathf.PI;

        float x = (512f / 2f) * Mathf.Cos(rad) + 960f;
        float y = (512f / 2f) * Mathf.Sin(rad) + 540f;

        Vector2 nextCenter = new Vector2(x, y);

        Vector2 nextCenterWorld = Camera.main.ScreenToWorldPoint(nextCenter);

        currentTarget = Instantiate(target, (Vector3)nextCenterWorld, Quaternion.identity);

        return nextCenter;
    }
}

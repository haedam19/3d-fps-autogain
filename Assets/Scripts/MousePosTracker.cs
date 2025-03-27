using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MousePosTracker : MonoBehaviour
{
    [SerializeField] TargetSpawner m_targetSpawner;

    [SerializeField] int targetNum;
    [SerializeField] float margin;

    float previousClickTime;

    Vector2 mousePos;
    Vector2 currentTargetCenter;
    float radius;

    Queue<Vector2> mousePosLog;
    Queue<Vector2> targetPosLog;

    private void Awake()
    {

    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.playing)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Input.mousePosition;
            float delta1 = (mousePos - currentTargetCenter).magnitude;
            float delta2 = Mathf.Max(delta1 - radius, 0f);

        }
    }

    public void RequestFirstSpawn()
    {
        currentTargetCenter = m_targetSpawner.Spawn();
    }

    public void ExportData()
    {

    }
}

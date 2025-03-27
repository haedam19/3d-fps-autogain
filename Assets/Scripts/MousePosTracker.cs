using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using TMPro;

public class MousePosTracker : MonoBehaviour
{
    [SerializeField] TargetSpawner m_targetSpawner;

    float previousClickTime;

    Vector2 mousePos;
    Vector2 targetPos;
    float radius;

    Queue<Vector2> mousePosLog;
    Queue<Vector2> targetPosLog;
    Queue<float> clickTimeLog;

    private void Awake()
    {

    }

    void Start()
    {
        mousePosLog = new Queue<Vector2>();
        targetPosLog = new Queue<Vector2>();
        clickTimeLog = new Queue<float>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.playing)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Input.mousePosition;
            // float delta1 = (mousePos - targetPos).magnitude;
            // float delta2 = Mathf.Max(delta1 - radius, 0f);

            mousePosLog.Enqueue(mousePos);
            targetPosLog.Enqueue(targetPos);
            clickTimeLog.Enqueue(Time.time - GameManager.Instance.startTime);

            GameManager.Instance.Trial++;

            targetPos = m_targetSpawner.Spawn();                
        }
    }

    public void RequestFirstSpawn()
    {
        targetPos = m_targetSpawner.Spawn();
    }

    public void ExportData()
    {
        /*
        float x = Random.Range(50f, 1870f);
        float y = Random.Range(50f, 1030f);

        float e_x = Random.value;
        float e_y = Random.value;
        */

        GameManager.Instance.inExporting = true;
        int count;
        if(mousePosLog.Count == targetPosLog.Count && mousePosLog.Count == clickTimeLog.Count)
        {
            count = mousePosLog.Count;
        }
        else
        {
            Debug.LogError("Length of log queues are not identical.");
            count = Mathf.Min(mousePosLog.Count, targetPosLog.Count, clickTimeLog.Count);
        }

        StringBuilder sb = new StringBuilder();

        Vector2 prevMousePos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        float prevClickTime = 0f;

        Vector2 mousePos;
        Vector2 targetPos;

        Vector2 moveDelta;
        Vector2 error;

        float clickTime;
        float timeDelta;

        string row = "mPos_x,mPos_y,tPos_x,tPos_y,delta_move_x,delta_move_y,error_x,error_y,click_time,delta_time\n";
        sb.Append(row);

        for (int i = 0; i < count; i++)
        {
            mousePos = mousePosLog.Dequeue();
            targetPos = targetPosLog.Dequeue();
            clickTime = clickTimeLog.Dequeue();

            moveDelta = mousePos - prevMousePos;
            error = mousePos - targetPos;
            timeDelta = clickTime - prevClickTime;

            row = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}\n",
                mousePos.x, mousePos.y,
                targetPos.x, targetPos.y,
                moveDelta.x, moveDelta.y,
                error.x, error.y,
                clickTime, timeDelta
            );
            sb.Append(row);
        }

        string filePath = GetPath();
        StreamWriter outStream = File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
        GameManager.Instance.inExporting = false;
    }

    string GetPath()
    {
    #if UNITY_EDITOR
        return Application.dataPath + "\\data.csv";
    #elif UNITY_STANDALONE_WIN
        return Application.persistentDataPath + "\\data.csv";
    #endif
    }
}

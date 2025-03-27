using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                return null;
            else
                return instance;
        }
    }

    [SerializeField] MousePosTracker m_mouseTracker;
    [SerializeField] TargetSpawner m_targetSpawner;

    public float startTime;
    public bool playing;
    public bool inExporting;

    public int goal;
    public int trial;
    public int Trial
    {
        get { return trial; }
        set 
        {
            trial = value;
            if (trial == goal)
                GameOver();
        }
    }

    void Awake()
    {
        playing = false;
        inExporting = false;
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Trial = 0;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing && Input.GetKeyDown(KeyCode.Space))
            GameStart();
        else if (!inExporting && Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void GameStart()
    {
        playing = true;
        Cursor.lockState = CursorLockMode.None;
        startTime = Time.time;
        Destroy(GameObject.Find("StartMessage"));
        m_mouseTracker.RequestFirstSpawn();
    }

    public void GameOver()
    {
        playing = false;
        Cursor.lockState = CursorLockMode.Locked;
        Destroy(m_targetSpawner.currentTarget);
        m_mouseTracker.ExportData();
    }
}

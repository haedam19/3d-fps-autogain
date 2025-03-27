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

    public int goal;
    public int trial;

    void Awake()
    {
        playing = false;
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        trial = 0;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing && Input.GetKeyDown(KeyCode.Space))
            GameStart();
    }

    public void GameStart()
    {
        playing = true;
        Cursor.lockState = CursorLockMode.None;
        startTime = Time.time;

        m_mouseTracker.RequestFirstSpawn();
    }

    public void GameOver()
    {

    }
}

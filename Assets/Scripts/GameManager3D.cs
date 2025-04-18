using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager3D : MonoBehaviour
{ 
    static GameManager3D instance;
    public static GameManager3D Instance
    {
        get
        {
            if (instance == null)
                return null;
            else
                return instance;
        }
    }

    [Header("SubManagers")]
    public EnvManager3D envManager;
    public TargetManager3D targetManager;
    public MousePosTracker3D mousePosTracker;

    [Header("GameStatus")]
    public bool playing;
    public bool inExporting;
    public float startTime;
    public ushort trialIndex;

    [Header("Statistics")]
    public int totalTrialCount;
    public int errorCount;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            playing = false;
            inExporting = false;
            envManager.Init();
            targetManager.Init();
            mousePosTracker.Init();

            trialIndex = 0;
            totalTrialCount = 0;
            errorCount = 0;
        }
        else
            Destroy(gameObject);
    }

    public void Start()
    {
        
    }

    /// <summary> Condition 단위의 측정 시작 메소드 </summary>
    public void TestStart()
    {
        playing = true;
        Cursor.lockState = CursorLockMode.None;
        startTime = Time.time;
        Destroy(GameObject.Find("StartMessage"));
    }

    public void Click()
    {

    }
}

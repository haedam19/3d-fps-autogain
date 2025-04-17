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

    [SerializeField] TargetManager3D m_targetManager;

    public float startTime;
    public bool playing;
    public bool inExporting;

    public ushort conditionIndex;
    public ushort trialIndex;

    public void Awake()
    {
        playing = false;
        inExporting = false;
        if (instance == null)
            instance = this;
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

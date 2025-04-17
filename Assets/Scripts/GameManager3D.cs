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

    public void Start()
    {
        
    }

    public void GameStart()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGManager : MonoBehaviour
{
    #region Singleton
    AGManager instance = null;

    public AGManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = this;
            }
            return instance;
        }
    }
    #endregion

    public enum GameState { Entrance, Standby, InBlock, InterBlock, Exit }

    AGTargetGenerator targetGenerator;

    AGTrialData lastTrial;
    AGTrialData curTrial;

    [SerializeField] int completeTrialCount = 0; // 완료된 Trial의 수
    [SerializeField] int totalTrialCount = 400; // 총 Trial의 수
    [SerializeField] int trialsPerBlock = 80; // 블록당 Trial의 수

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else if (instance == null)
        {
            instance = this;
        }

        curTrial = new AGTrialData();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGManager : MonoBehaviour
{
    #region Singleton
    static AGManager instance = null;

    public static AGManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            else
                return null;
        }
    }
    #endregion

    public enum GameState { Entrance, Standby, InBlock, InterBlock, Exit }
    public GameState currentState = GameState.Entrance; // 현재 게임 상태

    [SerializeField] AGTargetGenerator targetGenerator;
    [SerializeField] AGUIManager uiManager;
    [SerializeField] SimpleFirstPersonCamera camController;

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
        uiManager.ShowIndependentVariableSelectionUI();
    }

    public void StartAutoGainMode()
    {
        currentState = GameState.Standby;
        targetGenerator.GenerateNextTarget();
        camController.enabled = true; // 카메라 컨트롤러 활성화
    }

    public void StartReferenceMode()
    {
        currentState = GameState.Standby;
        targetGenerator.GenerateNextTarget();
        camController.enabled = true; // 카메라 컨트롤러 활성화
    }


    // Update is called once per frame
    void Update()
    {
        if (currentState == GameState.Entrance)
            return;

        // 마우스 클릭 시 다음 타겟 생성 (실험용)
        if (Input.GetMouseButtonDown(0))
        {
            targetGenerator.GenerateNextTarget();
            if ( currentState == GameState.InBlock)
            {
                uiManager.ShowSubmovementHUD(true);
            }
            currentState = GameState.InBlock;

        }
    }
}

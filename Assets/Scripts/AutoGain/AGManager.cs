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
    public GameState currentState = GameState.Entrance; // ���� ���� ����

    [SerializeField] AGTargetGenerator targetGenerator;
    [SerializeField] AGUIManager uiManager;
    [SerializeField] SimpleFirstPersonCamera camController;

    AGTrialData lastTrial;
    AGTrialData curTrial;

    [SerializeField] int completeTrialCount = 0; // �Ϸ�� Trial�� ��
    [SerializeField] int totalTrialCount = 400; // �� Trial�� ��
    [SerializeField] int trialsPerBlock = 80; // ��ϴ� Trial�� ��

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
        camController.enabled = true; // ī�޶� ��Ʈ�ѷ� Ȱ��ȭ
    }

    public void StartReferenceMode()
    {
        currentState = GameState.Standby;
        targetGenerator.GenerateNextTarget();
        camController.enabled = true; // ī�޶� ��Ʈ�ѷ� Ȱ��ȭ
    }


    // Update is called once per frame
    void Update()
    {
        if (currentState == GameState.Entrance)
            return;

        // ���콺 Ŭ�� �� ���� Ÿ�� ���� (�����)
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

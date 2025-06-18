using MouseLog;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class AGManager : MonoBehaviour
{
    private const double MinDblClickDist = 4.0; // minimum distance two clicks must be apart (filters double-clicks)

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

    public enum GainMode { REFERENCE, AUTOGAIN };
    public GainMode currentGainMode = GainMode.REFERENCE; // 현재 Gain 모드

    [SerializeField] AGTargetGenerator targetGenerator;
    [SerializeField] AGUIManager uiManager;
    [SerializeField] AGMouse agMouse;
    AutoGain autoGain;
    public static AutoGain AG { get { return Instance.autoGain; } }

    [SerializeField] int practiceTrialCount = 10; // 연습 Trial의 수 (시작 Trial 포함)
    [SerializeField] int totalTrialCount = 400; // 총 Trial의 수
    [SerializeField] int trialsPerBlock = 80; // 블록당 Trial의 수

    List<AGTrialData> trials; // 0번은 시작 Trial
    AGTrialData _tdata;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        currentState = GameState.Entrance;
        agMouse.Init();

        trials = new List<AGTrialData>(totalTrialCount);

        uiManager.ShowIndependentVariableSelectionUI();
    }

    /// <summary> AGMouse 객체를 생성해 Gain 모드를 설정하고 실험을 시작합니다. </summary>
    public void SetGainMode(GainMode gainMode)
    {
        currentGainMode = gainMode;
        currentState = GameState.Standby;
        agMouse.enabled = true;

        if (gainMode == GainMode.AUTOGAIN)
        {
            autoGain = new AutoGain();
            agMouse.useAutoGain = true;
        }
        else
        {
            autoGain = null;
            agMouse.useAutoGain = false;
        }

        targetGenerator.GenerateNextTarget();
    }

    public void StartTest()
    {
        AGTargetData targetData = targetGenerator.GenerateNextTarget();
        _tdata = new AGTrialData(trials.Count, trials.Count < practiceTrialCount, AGTargetData.Empty, targetData);
        currentState = GameState.Standby;
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

    /// <summary>
    /// AGMouse로부터 마우스 이동 이벤트를 받아 처리합니다.
    /// </summary>
    /// <param name="move"></param>
    public void MouseMove(MouseMove move)
    {
        Vector2 unityCoordCurrentMove = move.currentPos;
        Vector2 unityScreenCoordCurrentMove = unityCoordCurrentMove + new Vector2(Screen.width / 2, Screen.height / 2);
        PointR curPos = (PointR)unityScreenCoordCurrentMove;


    }

    /// <summary>
    /// 테스트 중 발생한 클릭 이벤트를 처리합니다.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="time"></param>
    public void MouseClick(Vector2 pos, long time)
    {
        // 블록 시작지점 또는 블록 중간에 클릭이 발생한 경우에는 처리 X
        if (currentState != GameState.Standby && currentState != GameState.InBlock)
            return;

        TimePointR clickTimePos = new TimePointR((PointR)(pos + new Vector2(Screen.width / 2, Screen.height / 2)), time);

        if (trials.Count == 0 || PointR.Distance((PointR)_tdata.Start, (PointR)clickTimePos) > MinDblClickDist)
            NextTrial(clickTimePos);
    }

    void NextTrial(TimePointR click)
    {
        if (currentState == GameState.Standby) // 시작 trial인 경우
        {
            if (!_tdata.TargetContains((PointR)click)) // click missed start target
            {
                DoError();
            }
            else // start first actual trial
            {
                trials.Add(_tdata); // 시작 Trial 기록

                AGTargetData nextAGTargetData;
                while(true)
                {
                    nextAGTargetData = targetGenerator.GenerateNextTarget();

                    if (!nextAGTargetData.IsEmpty())
                        break; // 유효한 타겟 생성
                    else
                        agMouse.ResetCameraRotation(); // 타겟 생성 실패 시 카메라 회전 초기화 후 재시도
                }
            }
        }
    }

    private void DoError()
    {
        // TODO: IMPLEMENT
    }

}

using MouseLog;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public enum GameState { Entrance, Standby, InTest, InterTest, Exit }
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

    List<AGTrialData> trials;
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
        agMouse.enabled = false;
        
        trials = new List<AGTrialData>(totalTrialCount);

        uiManager.ShowIndependentVariableSelectionUI();
    }

    /// <summary> AGMouse 객체를 생성해 Gain 모드를 설정하고 실험을 시작합니다. </summary>
    public void SetGainMode(GainMode gainMode)
    {
        currentGainMode = gainMode;
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

        StartTest();
    }

    public void StartTest()
    {
        agMouse.enabled = true;

        AGTargetData targetData = targetGenerator.GenerateNextTarget();
        _tdata = new AGTrialData(trials.Count, trials.Count < practiceTrialCount, AGTargetData.Empty, targetData);
        currentState = GameState.Standby;
    }

    public void StopTest(bool interrupted)
    {
        agMouse.enabled = false;
        _tdata = null;
        currentState = GameState.InterTest;
        uiManager.ShowStopMsgBox(interrupted);
    }

    public void FinishTest()
    {
        Debug.Log("Test Finished. Total Trials: " + trials.Count);
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.InTest)
            {
                StopTest(false);
            }
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
        if (currentState != GameState.Standby && currentState != GameState.InTest)
            return;

        TimePointR clickTimePos = new TimePointR((PointR)(pos + new Vector2(Screen.width / 2, Screen.height / 2)), time);

        if (_tdata.IsStartTrial || PointR.Distance((PointR)_tdata.Start, (PointR)clickTimePos) > MinDblClickDist)
            NextTrial(clickTimePos);
    }

    void NextTrial(TimePointR click)
    {
        string debugMsg = $"click: {(PointR)click}, target: {_tdata.ThisTarget.posR}";
        Debug.Log(debugMsg);

        if (currentState == GameState.Standby) // 시작 trial인 경우
        {
            if (!_tdata.TargetContains((PointR)click)) // click missed start target
            {
                DoError();
            }
            else // start first actual trial
            {
                AGTrialData lastTrial = _tdata;

                AGTargetData nextAGTargetData = targetGenerator.GenerateNextTarget();
                if (nextAGTargetData.IsEmpty())
                    StopTest(true); // 타겟 생성 실패 시 테스트 정지
                else
                {
                    _tdata = new AGTrialData(trials.Count, trials.Count < practiceTrialCount, lastTrial.ThisTarget, nextAGTargetData);
                    _tdata.Start = click;
                    currentState = GameState.InTest;
                }
            }
        }
        else if (currentState == GameState.InTest)
        {
            _tdata.End = click;
            _tdata.NormalizeTimes();
            trials.Add(_tdata);
            LogTrial(_tdata);
            if (_tdata.IsError)
                DoError();

            if (trials.Count >= totalTrialCount) // 모든 Trial이 끝난 경우
            {
                FinishTest();
                return;
            }

            AGTargetData nextAGTargetData = targetGenerator.GenerateNextTarget();
            if (nextAGTargetData.IsEmpty())
                StopTest(true); // 타겟 생성 실패 시 테스트 정지
            else
            {
                _tdata = new AGTrialData(trials.Count, trials.Count < practiceTrialCount, trials[trials.Count - 1].ThisTarget, nextAGTargetData);
                _tdata.Start = click;
            }
            
        }
    }

    private void DoError()
    {
        Debug.Log("Error!");
    }

    void LogTrial(AGTrialData tdata)
    {
        int index = trials.Count;
        Debug.Log($"Trial {index}/{totalTrialCount}\n" +
                  $"Start: {tdata.Start}, End: {tdata.End}, " +
                  $"Error: {tdata.IsError}");
    }

}

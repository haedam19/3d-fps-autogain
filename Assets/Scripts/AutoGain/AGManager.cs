using MouseLog;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
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
    [SerializeField] int totalTrialCount = 30; // 총 Trial의 수

    List<AGTrialData> trials;
    AGTrialData _tdata;

    float maxRawSpeed = 0f;
    float minRawSpeed = float.MaxValue;

    private string gameLogfilePath; // 게임 실행 관련 로그 기록 경로

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

#if UNITY_EDITOR
        gameLogfilePath = Application.dataPath + "/Log";
#elif UNITY_STANDALONE_WIN
        gameLogfilePath = Application.persistentDataPath;
#endif

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
            autoGain = new AutoGain(10.0);
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
        string filename = AGCSVExporter.GetTimestampedFilename();
        string path = Path.Combine(gameLogfilePath, filename);
        try
        {
            AGCSVExporter.ExportTrialsToCSV(trials, path);
            Debug.Log("CSV export success: " + path);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CSV export failed: " + ex.Message);
        }
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
    public void MouseMove(MouseMove move, long deltaTimeMs)
    {
        if (currentState != GameState.Standby && currentState != GameState.InTest)
            return;

        Vector2 unityCoordCurrentMove = move.currentPos;
        Vector2 unityScreenCoordCurrentMove = unityCoordCurrentMove + new Vector2(Screen.width / 2, Screen.height / 2);
        PointR curPos = (PointR)unityScreenCoordCurrentMove;

        if (_tdata != null && !_tdata.IsStartTrial && (currentState == GameState.Standby || currentState == GameState.InTest))
        {
            _tdata?.Movement.AddMove(new TimePointR(curPos, move.timeStamp));
            _tdata?.Movement.AddRawSpeed(new TimePointR(0.0, (double)move.gDelta.magnitude / (deltaTimeMs / 1000.0), move.timeStamp));
        }
            
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
                    _tdata.A = PointR.Distance((PointR)click, nextAGTargetData.posR);
                    _tdata.W = nextAGTargetData.w;
                    currentState = GameState.InTest;
                }
            }
        }
        else if (currentState == GameState.InTest)
        {
            _tdata.End = click;
            _tdata.NormalizeTimes();
            trials.Add(_tdata);
            // TODO: Update Gain
            
            //AGMovementData movementData = _tdata.Movement;
            //AGMovementData.Profiles profiles = movementData.CreateSmoothedProfiles();
            //foreach(PointR profile in profiles.RawVelocity)
            //{
            //    if ((float)profile.Y > maxRawSpeed)
            //        maxRawSpeed = (float)profile.Y;
            //    if ((float)profile.Y < minRawSpeed)
            //        minRawSpeed = (float)profile.Y;
            //    Debug.Log($"{profile.Y:F3}, Max: {maxRawSpeed:F3}, Min: {minRawSpeed:F3}");
            //}
            

            uiManager.UpdateStatusHUD(trials.Count, totalTrialCount, _tdata);
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
                _tdata.A = PointR.Distance((PointR)click, nextAGTargetData.posR);
                _tdata.W = nextAGTargetData.w;
            }
            
        }
    }

    private void DoError()
    {
#if UNITY_EDITOR
        Debug.Log("Error!");
#endif
    }

}

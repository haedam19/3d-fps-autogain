using MouseLog;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
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
    [HideInInspector] public AGViewerClient viewerClient; 
    AutoGain autoGain;
    public static AutoGain AG { get { return Instance.autoGain; } }

    [SerializeField] int practiceTrialCount = 10; // 연습 Trial의 수 (시작 Trial 포함)
    [SerializeField] int totalTrialCount = 300; // 총 Trial의 수
    
    List<AGTrialData> trials;
    AGTrialData _tdata;

    string continueSource; // 이전 실험결과에서 이어할 경우 이어 실행할 실험결과 폴더 이름(타임스탬프 형식으로 된 것)
    float maxRawSpeed = 0f;
    float minRawSpeed = float.MaxValue;

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

    public void SetContinueSource(string sourceFolderName)
    {
        continueSource = string.IsNullOrWhiteSpace(sourceFolderName) ? null : sourceFolderName.Trim();
    }

    /// <summary> AGMouse 객체를 생성해 Gain 모드를 설정하고 실험을 시작합니다. </summary>
    public void SetGainMode(GainMode gainMode)
    {
        currentGainMode = gainMode;
        agMouse.enabled = true;

        if (gainMode == GainMode.AUTOGAIN)
        {
            autoGain = new AutoGain(3.0, continueSource);
            agMouse.useAutoGain = true;
            if (viewerClient != null && viewerClient.IsConnected)
                SendAGInitDataToViewer();
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
        currentState = GameState.Exit;
        agMouse.enabled = false;
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string sessionLogPath = Path.Combine(ProjectPaths.AutoGainLogPath, timestamp);
        Directory.CreateDirectory(sessionLogPath);
        string filename = $"trial_results_{timestamp}.csv";
        string csvFullPath = Path.Combine(sessionLogPath, filename);
        try
        {
            AGCSVExporter.ExportTrialsToCSV(trials, csvFullPath);
            if(currentGainMode == GainMode.AUTOGAIN)
            {
                AG.ExportGainLogs(Path.Combine(sessionLogPath, $"gain_log_{timestamp}.csv"));
                AG.ExportFinalGainData(Path.Combine(sessionLogPath, $"final_gain_data_{timestamp}.json"));
            }
            
            uiManager.ShowEndMsgBox();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Result export failed: " + ex.Message);
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
            
            uiManager.UpdateStatusHUD(trials.Count, totalTrialCount, _tdata);
            if(currentGainMode == GainMode.AUTOGAIN)
            {
                AG.UpdateGainCurve(_tdata, out bool gainUpdated, out bool snapshotRecorded);

                if (viewerClient != null && viewerClient.IsConnected && gainUpdated)
                {
                    FullGainCurve gainData = AG.GetFullGainCurveJsonData();
                    string gainDataJson = JsonUtility.ToJson(gainData);
                    _ = viewerClient.SendTextAsync(gainDataJson);

                    if (snapshotRecorded)
                    {
                        GainSnapshot gainSnapshot = AG.GetLastGainSnapshotJsonData();
                        string gainSnapshotJson = JsonUtility.ToJson(gainSnapshot);
                        _ = viewerClient.SendTextAsync(gainSnapshotJson);
                    }
                }
            }
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

    /// <summary>
    /// AutoGain의 초기 데이터를 Viewer에 전송합니다.
    /// </summary>
    private async void SendAGInitDataToViewer()
    {
        if (viewerClient == null || !viewerClient.IsConnected || AG == null)
            return;

        MetaData metaData = AG.GetMetaData();
        await viewerClient.SendTextAsync(JsonUtility.ToJson(metaData));

        FullGainCurve gainData = AG.GetFullGainCurveJsonData();
        await viewerClient.SendTextAsync(JsonUtility.ToJson(gainData));

        List<GainSnapshot> snapshots = AG.GetGainSnapshotJsonDataList();
        foreach (GainSnapshot snapshot in snapshots)
            await viewerClient.SendTextAsync(JsonUtility.ToJson(snapshot));
    }

    private void DoError()
    {
#if UNITY_EDITOR
        Debug.Log("Error!");
#endif
    }

}

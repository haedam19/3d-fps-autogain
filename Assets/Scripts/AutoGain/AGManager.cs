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
    public GameState currentState = GameState.Entrance; // ���� ���� ����

    public enum GainMode { REFERENCE, AUTOGAIN };
    public GainMode currentGainMode = GainMode.REFERENCE; // ���� Gain ���

    [SerializeField] AGTargetGenerator targetGenerator;
    [SerializeField] AGUIManager uiManager;
    [SerializeField] AGMouse agMouse;
    AutoGain autoGain;
    public static AutoGain AG { get { return Instance.autoGain; } }

    [SerializeField] int practiceTrialCount = 10; // ���� Trial�� �� (���� Trial ����)
    [SerializeField] int totalTrialCount = 400; // �� Trial�� ��

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

    /// <summary> AGMouse ��ü�� ������ Gain ��带 �����ϰ� ������ �����մϴ�. </summary>
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
    /// AGMouse�κ��� ���콺 �̵� �̺�Ʈ�� �޾� ó���մϴ�.
    /// </summary>
    /// <param name="move"></param>
    public void MouseMove(MouseMove move)
    {
        Vector2 unityCoordCurrentMove = move.currentPos;
        Vector2 unityScreenCoordCurrentMove = unityCoordCurrentMove + new Vector2(Screen.width / 2, Screen.height / 2);
        PointR curPos = (PointR)unityScreenCoordCurrentMove;


    }

    /// <summary>
    /// �׽�Ʈ �� �߻��� Ŭ�� �̺�Ʈ�� ó���մϴ�.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="time"></param>
    public void MouseClick(Vector2 pos, long time)
    {
        // ��� �������� �Ǵ� ��� �߰��� Ŭ���� �߻��� ��쿡�� ó�� X
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

        if (currentState == GameState.Standby) // ���� trial�� ���
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
                    StopTest(true); // Ÿ�� ���� ���� �� �׽�Ʈ ����
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

            if (trials.Count >= totalTrialCount) // ��� Trial�� ���� ���
            {
                FinishTest();
                return;
            }

            AGTargetData nextAGTargetData = targetGenerator.GenerateNextTarget();
            if (nextAGTargetData.IsEmpty())
                StopTest(true); // Ÿ�� ���� ���� �� �׽�Ʈ ����
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

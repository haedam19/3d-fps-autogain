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
    [SerializeField] int trialsPerBlock = 80; // ��ϴ� Trial�� ��

    List<AGTrialData> trials; // 0���� ���� Trial
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

    /// <summary> AGMouse ��ü�� ������ Gain ��带 �����ϰ� ������ �����մϴ�. </summary>
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
        if (currentState != GameState.Standby && currentState != GameState.InBlock)
            return;

        TimePointR clickTimePos = new TimePointR((PointR)(pos + new Vector2(Screen.width / 2, Screen.height / 2)), time);

        if (trials.Count == 0 || PointR.Distance((PointR)_tdata.Start, (PointR)clickTimePos) > MinDblClickDist)
            NextTrial(clickTimePos);
    }

    void NextTrial(TimePointR click)
    {
        if (currentState == GameState.Standby) // ���� trial�� ���
        {
            if (!_tdata.TargetContains((PointR)click)) // click missed start target
            {
                DoError();
            }
            else // start first actual trial
            {
                trials.Add(_tdata); // ���� Trial ���

                AGTargetData nextAGTargetData;
                while(true)
                {
                    nextAGTargetData = targetGenerator.GenerateNextTarget();

                    if (!nextAGTargetData.IsEmpty())
                        break; // ��ȿ�� Ÿ�� ����
                    else
                        agMouse.ResetCameraRotation(); // Ÿ�� ���� ���� �� ī�޶� ȸ�� �ʱ�ȭ �� ��õ�
                }
            }
        }
    }

    private void DoError()
    {
        // TODO: IMPLEMENT
    }

}

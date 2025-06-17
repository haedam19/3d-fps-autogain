using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using MouseLog;
using UnityEngine;

public class GameManager3D : MonoBehaviour
{
    private const double MinDblClickDist = 4.0; // minimum distance two clicks must be apart (filters double-clicks)

    #region Singleton Instance
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
    #endregion

    public enum GameState {
        InterCondition, // 측정 중이 아닌 상태
        StartTrial,     // Start Trial 입력 대기 중
        Measuring       // Condition 내에서 측정 시행 중
    }

    [SerializeField] private GameState currentState; // 현재 게임 상태

    private string gameLogfilePath; // 게임 실행 관련 로그 기록 경로

    [Header("SubSystems")]
    public TargetManager3D targetManager;
    public UIManager3D uiManager;
    public FittsMouseTracker mouseTracker;

    [Header("Statistics")]
    public int totalTrialCount;

    #region MouseLogData
    private SessionData _sdata; // the whole session (one test); holds conditions in order
    private ConditionData _cdata; // the current condition; retrieved from the session
    private TrialData _tdata; // the current trial; retrieved from the condition

    public SessionData Session { get { return _sdata; } }
    public ConditionData Condition { get { return _cdata; } }
    public TrialData TrialData { get { return _tdata; } }
    #endregion

    #region file
    public bool expComplete; // 측정이 끝까지 진행되었는지 체크
    private string _fileNoExt; // full path and filename without extension
    private XmlTextWriter _writer; // XML writer -- uses _fileNoExt.xml

    #endregion

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        currentState = GameState.InterCondition;

        uiManager.Init();
        targetManager.Init();

        // 로그 파일 경로 및 이름 설정 & xml 로그 준비
#if UNITY_EDITOR
        gameLogfilePath = Application.dataPath + "/Log";
#elif UNITY_STANDALONE_WIN
        gameLogfilePath = Application.persistentDataPath;
#endif
        //session_onfig.json 로드 -> _sdata, _cdata, _tdata 초기화
        SessionConfiguration sessionconfig = LoadData();

        // 남아있는 .tmp 파일 있으면 정리. (이전 실행 중에 비정상 종료되어 남아있는 파일)
        string leftoverTmp = Directory.GetFiles(gameLogfilePath, "*.tmp").FirstOrDefault();
        if (leftoverTmp != null)
            File.Delete(leftoverTmp);

        // FilenameBase: s{subject id}_{1D / 2D}_{mnone / nomet}
        _fileNoExt = string.Format("{0}\\{1}__{2}", gameLogfilePath, _sdata.FilenameBase, Environment.TickCount);
        _writer = new XmlTextWriter(_fileNoExt + ".tmp", Encoding.UTF8); // 처음엔 .tmp로 저장. 측정이 정상적으로 종료되었을 때만 .xml로 변경
        _writer.Formatting = Formatting.Indented;
        _sdata.WriteXmlHeader(_writer);
        expComplete = false;

        // 타이머 초기화
        Timer.Reset();

        mouseTracker.Init();
        mouseTracker.enabled = false;

        totalTrialCount = 0;

        uiManager.ShowSessionStartMsgBox(sessionconfig);
    }

    public void TestStart()
    {
        targetManager.SetActiveCondition(0); // 첫 번째 Condition의 타겟 세팅
        _tdata.ThisTarget.TargetOn(); // 첫 번째 타겟 켜기
        currentState = GameState.StartTrial; // 상태 변경
        mouseTracker.enabled = true; // 마우스 트래커 활성화
    }

    #region Mouse Event Handling

    public void MouseClick()
    {

    }

    public void MouseClick(Vector2 pos, long time, bool rayHitFlag, RaycastHit hitInfo)
    {
        if (_cdata == null || currentState == GameState.InterCondition)
            return;

        // TimePointR로 변환하면서 좌상단 원점 좌표계로 변환
        TimePointR clickTimePos = new TimePointR((PointR)(pos + new Vector2(Screen.width / 2, Screen.height / 2)), time);

        // Debug.Log("Click: " + $"{clickTimePos.X}, {clickTimePos.Y}");
        // Debug.Log($"{_tdata.ThisTarget.CenterP.X}, {_tdata.ThisTarget.CenterP.Y}");

        // 시작 Trial이면 바로 NextTrial 호출, 아닐 경우엔 더블클릭 방지 연산 수행 후 NextTrial 호출
        if (_tdata.IsStartAreaTrial || PointR.Distance((PointR)_tdata.Start, (PointR)clickTimePos) > MinDblClickDist)
            NextTrial(clickTimePos, rayHitFlag, hitInfo);
    }

    public void MouseMove(MouseMove move)
    {
        Vector2 unityCoordCurrentMove = move.currentPos;
        Vector2 unityScreenCoordCurrentMove = unityCoordCurrentMove + new Vector2(Screen.width / 2, Screen.height / 2);
        PointR curPos = (PointR)unityScreenCoordCurrentMove;

        // only record moves when we are within a trial
        if (_tdata != null && !_tdata.IsStartAreaTrial && currentState != GameState.InterCondition) 
        {
            _tdata.Movement.AddMove(new TimePointR(curPos, move.timeStamp));
        }
    }

    void NextTrial(TimePointR click, bool rayHitFlag, RaycastHit hitInfo)
    {
        if (_tdata.IsStartAreaTrial) // 시작 trial인 경우
        {
            if (!_tdata.TargetContains((PointR)click)) // click missed start target
            {
                DoError();
            }
            else // start first actual trial
            {
                _tdata = _cdata[1]; // trial number 1
                _tdata.LastTarget.TargetOff(); // 지난 타겟 꺼짐
                _tdata.ThisTarget.TargetOn(); // 현재 타겟 켜짐
                _tdata.Start = click; // 시작 좌표 및 시간 기록
                currentState = GameState.Measuring; // 상태 변경
            }
        }
        else if (_tdata.Number < _cdata.NumTrials) // 동일 condition 내 다음 trial로 진행
        {
            _tdata.End = click;
            _tdata.NormalizeTimes();
            if (_tdata.IsError)
                DoError();

            _tdata = _cdata[_tdata.Number + 1];
            _tdata.LastTarget.TargetOff(); // 지난 타겟 꺼짐
            _tdata.ThisTarget.TargetOn(); // 현재 타겟 켜짐
            _tdata.Start = click; // 시작 좌표 및 시간 기록
        }
        else // Condition 종료. 
        {
            _tdata.End = click;
            _tdata.NormalizeTimes();
            if (_tdata.IsError)
                DoError();

            _cdata.WriteXmlHeader(_writer); // write out the condition and its trials to XML
            currentState = GameState.InterCondition;
            mouseTracker.enabled = false; // 마우스 트래커 비활성화
            UIManager3D.Instance.ShowConditionEndMsgBox(_cdata.Index + 1, _sdata.NumTotalConditions);
        }
    }

    public void NextCondition()
    {
        //다음 Condition으로 넘어가거나 Session 종료.
        if (_cdata.Index + 1 == _sdata.NumTotalConditions) // 마지막 Condition인 경우
        {
            // Session 종료
            _sdata.WriteXmlFooter(_writer);
            expComplete = true; // 측정 완료
            UIManager3D.Instance.ShowSessionEndMsgBox();
        }
        else // 다음 Condition으로 넘어감
        {
            currentState = GameState.StartTrial;
            mouseTracker.enabled = true;
            _cdata = _sdata[ _cdata.Index + 1];
            _tdata = _cdata[0];
            targetManager.SetActiveCondition(_cdata.Index); // 타겟 매니저에 현재 Condition 설정
            _tdata.ThisTarget.TargetOn(); // 첫 번째 타겟 켜기
        }
    }

    #endregion

    private void DoError()
    {
        // TODO: IMPLEMENT
    }

    /// <summary> config.json을 읽어 session 세팅. 실패하면 프로그램 종료. </summary>
    private SessionConfiguration LoadData()
    {
        SessionConfiguration sessionConfig = null;
        string json = null;
        json = File.ReadAllText(Path.Combine(Application.dataPath, "Json", "session_config.json"));
        string fileName = string.Format("ConfigLoadLog_{0}_{1}.txt", DateTime.Now.ToString("yyyy-MM-dd"), Environment.TickCount);

        if (json != null)
        {
            sessionConfig = JsonUtility.FromJson<SessionConfiguration>(json);
            if (sessionConfig.isValid())
            {
                // config 이용해 SessionData, ConditionData, TrialData 초기화
                _sdata = new SessionData(sessionConfig.subject, sessionConfig.isCircular, new ScreenData(Screen.width, Screen.height), sessionConfig.a, sessionConfig.w, null, 100.0, 200.0, sessionConfig.trials, sessionConfig.practice);
                _cdata = _sdata[0]; // first overall condition
                _tdata = _cdata[0]; // first trial is special start-area trial at index 0
                totalTrialCount = sessionConfig.trials;
                using (StreamWriter sw = new StreamWriter(Path.Combine(gameLogfilePath, fileName), true))
                    sw.WriteLine(json);

            }
            else
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(gameLogfilePath, fileName), true))
                    sw.WriteLine("Invalid config");
                Application.Quit();
            }
        }
        else
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(gameLogfilePath, fileName), true))
                sw.WriteLine("Failed to load session_config.json");
            Application.Quit();
        }

        return sessionConfig;
    }

    void OnApplicationQuit()
    {
        // 측정이 제대로 완료 되었다면 앱 종료시 .tmp로 저장된 기록을 xml로 옮기고 .tmp는 정리
        if (expComplete)
            FinalizeXmlWriter();
    }

    private void FinalizeXmlWriter()
    {
        if (_writer != null)
        {
            try
            {
                string tmpPath = _fileNoExt + ".tmp";
                string finalPath = _fileNoExt + ".xml";

                if (File.Exists(tmpPath))
                    File.Move(tmpPath, finalPath); // rename only if fully completed
            }
            catch (Exception ex)
            {
                Debug.LogWarning("XML 정리 중 오류: " + ex.Message);
            }
            finally
            {
                _writer = null;
            }
        }
    }

}

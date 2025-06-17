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
        InterCondition, // ���� ���� �ƴ� ����
        StartTrial,     // Start Trial �Է� ��� ��
        Measuring       // Condition ������ ���� ���� ��
    }

    [SerializeField] private GameState currentState; // ���� ���� ����

    private string gameLogfilePath; // ���� ���� ���� �α� ��� ���

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
    public bool expComplete; // ������ ������ ����Ǿ����� üũ
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

        // �α� ���� ��� �� �̸� ���� & xml �α� �غ�
#if UNITY_EDITOR
        gameLogfilePath = Application.dataPath + "/Log";
#elif UNITY_STANDALONE_WIN
        gameLogfilePath = Application.persistentDataPath;
#endif
        //session_onfig.json �ε� -> _sdata, _cdata, _tdata �ʱ�ȭ
        SessionConfiguration sessionconfig = LoadData();

        // �����ִ� .tmp ���� ������ ����. (���� ���� �߿� ������ ����Ǿ� �����ִ� ����)
        string leftoverTmp = Directory.GetFiles(gameLogfilePath, "*.tmp").FirstOrDefault();
        if (leftoverTmp != null)
            File.Delete(leftoverTmp);

        // FilenameBase: s{subject id}_{1D / 2D}_{mnone / nomet}
        _fileNoExt = string.Format("{0}\\{1}__{2}", gameLogfilePath, _sdata.FilenameBase, Environment.TickCount);
        _writer = new XmlTextWriter(_fileNoExt + ".tmp", Encoding.UTF8); // ó���� .tmp�� ����. ������ ���������� ����Ǿ��� ���� .xml�� ����
        _writer.Formatting = Formatting.Indented;
        _sdata.WriteXmlHeader(_writer);
        expComplete = false;

        // Ÿ�̸� �ʱ�ȭ
        Timer.Reset();

        mouseTracker.Init();
        mouseTracker.enabled = false;

        totalTrialCount = 0;

        uiManager.ShowSessionStartMsgBox(sessionconfig);
    }

    public void TestStart()
    {
        targetManager.SetActiveCondition(0); // ù ��° Condition�� Ÿ�� ����
        _tdata.ThisTarget.TargetOn(); // ù ��° Ÿ�� �ѱ�
        currentState = GameState.StartTrial; // ���� ����
        mouseTracker.enabled = true; // ���콺 Ʈ��Ŀ Ȱ��ȭ
    }

    #region Mouse Event Handling

    public void MouseClick()
    {

    }

    public void MouseClick(Vector2 pos, long time, bool rayHitFlag, RaycastHit hitInfo)
    {
        if (_cdata == null || currentState == GameState.InterCondition)
            return;

        // TimePointR�� ��ȯ�ϸ鼭 �»�� ���� ��ǥ��� ��ȯ
        TimePointR clickTimePos = new TimePointR((PointR)(pos + new Vector2(Screen.width / 2, Screen.height / 2)), time);

        // Debug.Log("Click: " + $"{clickTimePos.X}, {clickTimePos.Y}");
        // Debug.Log($"{_tdata.ThisTarget.CenterP.X}, {_tdata.ThisTarget.CenterP.Y}");

        // ���� Trial�̸� �ٷ� NextTrial ȣ��, �ƴ� ��쿣 ����Ŭ�� ���� ���� ���� �� NextTrial ȣ��
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
        if (_tdata.IsStartAreaTrial) // ���� trial�� ���
        {
            if (!_tdata.TargetContains((PointR)click)) // click missed start target
            {
                DoError();
            }
            else // start first actual trial
            {
                _tdata = _cdata[1]; // trial number 1
                _tdata.LastTarget.TargetOff(); // ���� Ÿ�� ����
                _tdata.ThisTarget.TargetOn(); // ���� Ÿ�� ����
                _tdata.Start = click; // ���� ��ǥ �� �ð� ���
                currentState = GameState.Measuring; // ���� ����
            }
        }
        else if (_tdata.Number < _cdata.NumTrials) // ���� condition �� ���� trial�� ����
        {
            _tdata.End = click;
            _tdata.NormalizeTimes();
            if (_tdata.IsError)
                DoError();

            _tdata = _cdata[_tdata.Number + 1];
            _tdata.LastTarget.TargetOff(); // ���� Ÿ�� ����
            _tdata.ThisTarget.TargetOn(); // ���� Ÿ�� ����
            _tdata.Start = click; // ���� ��ǥ �� �ð� ���
        }
        else // Condition ����. 
        {
            _tdata.End = click;
            _tdata.NormalizeTimes();
            if (_tdata.IsError)
                DoError();

            _cdata.WriteXmlHeader(_writer); // write out the condition and its trials to XML
            currentState = GameState.InterCondition;
            mouseTracker.enabled = false; // ���콺 Ʈ��Ŀ ��Ȱ��ȭ
            UIManager3D.Instance.ShowConditionEndMsgBox(_cdata.Index + 1, _sdata.NumTotalConditions);
        }
    }

    public void NextCondition()
    {
        //���� Condition���� �Ѿ�ų� Session ����.
        if (_cdata.Index + 1 == _sdata.NumTotalConditions) // ������ Condition�� ���
        {
            // Session ����
            _sdata.WriteXmlFooter(_writer);
            expComplete = true; // ���� �Ϸ�
            UIManager3D.Instance.ShowSessionEndMsgBox();
        }
        else // ���� Condition���� �Ѿ
        {
            currentState = GameState.StartTrial;
            mouseTracker.enabled = true;
            _cdata = _sdata[ _cdata.Index + 1];
            _tdata = _cdata[0];
            targetManager.SetActiveCondition(_cdata.Index); // Ÿ�� �Ŵ����� ���� Condition ����
            _tdata.ThisTarget.TargetOn(); // ù ��° Ÿ�� �ѱ�
        }
    }

    #endregion

    private void DoError()
    {
        // TODO: IMPLEMENT
    }

    /// <summary> config.json�� �о� session ����. �����ϸ� ���α׷� ����. </summary>
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
                // config �̿��� SessionData, ConditionData, TrialData �ʱ�ȭ
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
        // ������ ����� �Ϸ� �Ǿ��ٸ� �� ����� .tmp�� ����� ����� xml�� �ű�� .tmp�� ����
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
                Debug.LogWarning("XML ���� �� ����: " + ex.Message);
            }
            finally
            {
                _writer = null;
            }
        }
    }

}

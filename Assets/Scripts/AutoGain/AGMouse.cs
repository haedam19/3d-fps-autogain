using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity Input System을 이용한 1인칭 카메라 회전 스크립트입니다.
/// 별도의 플레이어 바디 없이 카메라 자체를 상하좌우 회전시킵니다.
/// </summary>
public class AGMouse : MonoBehaviour
{
    [Tooltip("실험을 위해 마우스 궤적 및 입력을 기록합니다.\n단위 테스트 중에는 false로 설정하세요.")]
    public bool recordingMode = true;
    [Tooltip("AutoGain 사용 여부를 설정합니다")]
    public bool useAutoGain = true;


    private Mouse Mouse { get { return Mouse.current; } }
    private float _d; // pixel per unit 딱 1일 때 기준 화면 가득 채우는 가상 평면까지의 거리

    // Fields for Update Method. 마우스 입력 데이터 처리에 사용.
    private Vector2 _delta; // InputSystem을 통해 받은 delta 값
    private Vector2 _gDelta; // Gain Function 처리 후 delta 값 
    private Vector2 _lastPos; // 이전 프레임 마우스 커서 위치
    private Vector2 _currentPos; // 잠정적인 마우스 커서 위치
    private bool _isClicked;

    private float pitch = 0f; // 피치(X축 회전)
    private float yaw = 0f; // 요(Y축 회전)
    private const float sensitivityInverseScaler = 100f; // constSensitivity로 실제 sensitivity를 산출하기 위한 역스케일링 값
    [Range(1, 50), Tooltip("정수 단위로 스케일링된 감도입니다. 실제 감도는 표시된 감도의 1/100입니다.")]
    public int sensitivity = 10; // 정수 단위로 스케일 업 된 sensitivity 값임.
    public float ConstSensitivity { get { return (float)sensitivity / sensitivityInverseScaler; } } // 마우스 감도 (회전)

    private long _curTime;
    private long _lastTime;

    int inputCount = 0; // 입력 횟수 카운트 (디버깅용)
    float maxSpeed = 0f; // 최대 속도 (디버깅용)
    float minSpeed = float.MaxValue; // 최소 속도 (디버깅용)
    float averageSpeed = 0f;


    #region Initialization and Reset Methods
    private void Awake()
    {
        // 레코딩 모드 아닐 때 (단위 테스트 등)
        if (!recordingMode)
        {
            Init(); // 초기화해줄 게임 매니저가 없을 수 있어 직접 초기화
        }
    }

    public void Init()
    {
        // _d만큼 떨어진 거리에 width * height 크기의 가상 스크린이 있으면 화면을 정확히 채움
        _d = Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
    }

    private void OnEnable()
    {
        ResetCameraRotation();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _delta = Vector2.zero;
        _lastPos = Vector2.zero;
        _currentPos = Vector2.zero;
        _isClicked = false;

        _curTime = Timer.Time;
        _lastTime = Timer.Time;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion

    void Update()
    {
        _delta = Mouse.delta.ReadValue();
        
        _lastTime = _curTime;
        _curTime = Timer.Time;
        long deltaTime = _curTime - _lastTime;

        /*
        float inputSpeed = _delta.magnitude / (deltaTime / 1000f);
        if (inputSpeed > 0f)
        {
            inputCount++;
            maxSpeed = Mathf.Max(maxSpeed, inputSpeed);
            minSpeed = Mathf.Min(minSpeed, inputSpeed);
            averageSpeed = (averageSpeed * (inputCount - 1) + inputSpeed) / inputCount;
            Debug.Log($"Current Speed: {inputSpeed}, Max Speed: {maxSpeed:F2}, Min Speed: {minSpeed:F2}, Average Speed: {averageSpeed:F2}");
        }
        */
        

        double deltaYaw, deltaPitch;
        if (useAutoGain)
            AGManager.AG.getTranslatedValue(_delta.x, _delta.y, deltaTime, out deltaYaw, out deltaPitch);
        else
        {
            deltaYaw = _delta.x * ConstSensitivity;// * (float)deltaTime / 1000f;
            deltaPitch = -_delta.y * ConstSensitivity;// * (float)deltaTime / 1000f;
        }
        

        // 1. 카메라 회전
        yaw += (float)deltaYaw;
        pitch += (float)deltaPitch;
        yaw = Mathf.Clamp(yaw, -60f, 60f); // 요 회전 제한
        pitch = Mathf.Clamp(pitch, -60f, 60f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 2. 평면 교차 계산
        Vector3 origin = transform.position;
        Vector3 dir = transform.forward;
        float t = (_d - origin.z) / dir.z;
        Vector3 intersection = origin + dir * t;

        _lastPos = _currentPos;
        _currentPos = new Vector2(intersection.x, intersection.y);
        _isClicked = Mouse.press.wasPressedThisFrame;

        if (!recordingMode) return;

        if (_delta.sqrMagnitude > 0)
            AGManager.Instance.MouseMove(new MouseMove(_delta, _lastPos, _currentPos, _curTime));
        if (_isClicked)
            AGManager.Instance.MouseClick(_currentPos, _curTime);
    }

    public void ResetCameraRotation()
    {
        pitch = 0f;
        yaw = 0f;
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
}

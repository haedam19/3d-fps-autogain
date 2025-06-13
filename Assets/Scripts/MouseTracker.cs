using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public struct MouseMove
{
    public Vector2 gDelta;
    public Vector2 lastPos;
    public Vector2 currentPos;
    public long timeStamp;

    public MouseMove(Vector2 gDelta, Vector2 lastPos, Vector2 currentPos, long time)
    {
        this.gDelta = gDelta;
        this.lastPos = lastPos;
        this.currentPos = currentPos;
        this.timeStamp = time;
    }

}

public class MouseTracker : MonoBehaviour
{
    // GameManager3D가 없는 상태에서 단위테스트시 true로 설정
    [SerializeField] bool unitTest = false;

    bool sensitivitySetting = true;
    [SerializeField] GameObject sensitivityView;
    [SerializeField] TMP_Text sensitivityText;

    private Mouse Mouse { get { return Mouse.current; } }
    public ControlDisplayGain gain;
    private float _d;

    // Fields for Update Method. 마우스 입력 데이터 처리에 사용.
    private Vector2 _delta; // InputSystem을 통해 받은 delta 값
    private Vector2 _gDelta; // Gain Function 처리 후 delta 값 
    private Vector2 _lastPos; // 이전 프레임 마우스 커서 위치
    private Vector2 _currentPos; // 잠정적인 마우스 커서 위치
    private bool _isClicked;


    private float yaw = 0f;
    private float pitch = 0f;
    public float sensitivity = 0.1f;

    public void Awake()
    {
        if (unitTest)
            Init(); // 초기화해줄 게임 매니저가 없기 때문에 직접 초기화
    }

    public void Init()
    {
        // _d만큼 떨어진 거리에 width * hight크기의 가상 스크린이 있으면 화면을 정확히 채움
        _d = Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
        gain = new ControlDisplayGain(ControlDisplayGain.Type.Univariate, 1f);
    }

    private void OnEnable()
    {
        transform.rotation = Quaternion.identity; // 카메라 회전 초기화
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _delta = Vector2.zero;
        _lastPos = Vector2.zero;
        _currentPos = Vector2.zero;
        _isClicked = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if(sensitivitySetting)
        {
            if(Input.GetKey(KeyCode.Space))
            {
                sensitivitySetting = false;
                Destroy(sensitivityView);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.A))
                    sensitivity -= 0.01f;
                if (Input.GetKeyDown(KeyCode.D))
                    sensitivity += 0.01f;
                sensitivity = Mathf.Clamp(sensitivity, 0.01f, 0.5f);
                sensitivityText.text = sensitivity.ToString("F2");
            }
            
        }


        _delta = Mouse.delta.ReadValue();

        // 1. 카메라 회전
        yaw += _delta.x * sensitivity;
        pitch -= _delta.y * sensitivity;
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

        if (unitTest || sensitivitySetting ) return;

        if (_delta.sqrMagnitude > 0)
            GameManager3D.Instance.MouseMove(new MouseMove(_delta, _lastPos, _currentPos, Timer.Time));

        if (_isClicked)
        {
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(transform.position, transform.forward, out hitInfo, 1e3f, LayerMask.NameToLayer("Target"));
            GameManager3D.Instance.MouseClick(_currentPos, Timer.Time, hit, hitInfo);
        }
    }
}
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

    private Mouse Mouse { get { return Mouse.current; } }
    public ControlDisplayGain gain;
    private float _d;

    // Fields for Update Method. 마우스 입력 데이터 처리에 사용.
    private Vector2 _delta; // InputSystem을 통해 받은 delta 값
    private Vector2 _gDelta; // Gain Function 처리 후 delta 값 
    private Vector2 _lastPos; // 이전 프레임 마우스 커서 위치
    private Vector2 _currentPos; // 잠정적인 마우스 커서 위치
    private bool _isClicked;

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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _delta = Vector2.zero;
        _lastPos = new Vector2(Screen.width / 2, Screen.height / 2);
        _currentPos = new Vector2(Screen.width / 2, Screen.height / 2);
        _isClicked = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // Get mouse input data
        _delta = Mouse.delta.ReadValue(); // WM_INPUT 메시지 이용, count 단위로 측정
        _gDelta = gain.GainedDelta(_delta); // CD Gain 적용한 delta, subpixel까지 계산됨(소수점 아래 단위 측정)
        _lastPos = _currentPos;
        _currentPos += _gDelta;
        _isClicked = Mouse.press.wasPressedThisFrame;

        // 카메라가 잠정적 커서 위치를 바라보도록 회전
        transform.LookAt(new Vector3(_currentPos.x, _currentPos.y, _d));

        if (unitTest) return;

        // 마우스 움직임 이벤트 발생
        if (_delta.sqrMagnitude > 0)
        {
            GameManager3D.Instance.MouseMove(new MouseMove(_gDelta, _lastPos, _currentPos, Timer.Time));
            // int currentCond = GameManager3D.Instance.Session.condIdx;
            // GameManager3D.Instance.Session._conditions[currentCond].AddMove(_currentPos);
        }

        // 클릭 이벤트 발생
        if (_isClicked)
        {
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(transform.position
                , transform.forward
                , out hitInfo
                , 1e3f, LayerMask.NameToLayer("Target"));
            GameManager3D.Instance.MouseClick(_currentPos, Timer.Time, hit, hitInfo);
        }
    }
}
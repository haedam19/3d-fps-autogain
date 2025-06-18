using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity Input System�� �̿��� 1��Ī ī�޶� ȸ�� ��ũ��Ʈ�Դϴ�.
/// ������ �÷��̾� �ٵ� ���� ī�޶� ��ü�� �����¿� ȸ����ŵ�ϴ�.
/// </summary>
public class AGMouse : MonoBehaviour
{
    [Tooltip("������ ���� ���콺 ���� �� �Է��� ����մϴ�.\n���� �׽�Ʈ �߿��� false�� �����ϼ���.")]
    public bool recordingMode = true;
    [Tooltip("AutoGain ��� ���θ� �����մϴ�")]
    public bool useAutoGain = true;


    private Mouse Mouse { get { return Mouse.current; } }
    private float _d; // pixel per unit �� 1�� �� ���� ȭ�� ���� ä��� ���� �������� �Ÿ�

    // Fields for Update Method. ���콺 �Է� ������ ó���� ���.
    private Vector2 _delta; // InputSystem�� ���� ���� delta ��
    private Vector2 _gDelta; // Gain Function ó�� �� delta �� 
    private Vector2 _lastPos; // ���� ������ ���콺 Ŀ�� ��ġ
    private Vector2 _currentPos; // �������� ���콺 Ŀ�� ��ġ
    private bool _isClicked;

    private float pitch = 0f; // ��ġ(X�� ȸ��)
    private float yaw = 0f; // ��(Y�� ȸ��)
    public float sensitivity = 10f; // ���콺 ���� (ȸ��)

    private long _curTime;
    private long _lastTime;

    #region Initialization and Reset Methods
    private void Awake()
    {
        // ���ڵ� ��� �ƴ� �� (���� �׽�Ʈ ��)
        if (!recordingMode)
        {
            Init(); // �ʱ�ȭ���� ���� �Ŵ����� ���� �� �־� ���� �ʱ�ȭ
        }
    }

    public void Init()
    {
        // _d��ŭ ������ �Ÿ��� width * height ũ���� ���� ��ũ���� ������ ȭ���� ��Ȯ�� ä��
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

        double deltaYaw, deltaPitch;
        if (useAutoGain)
            AGManager.AG.getTranslatedValue(_delta.x, _delta.y, deltaTime, out deltaYaw, out deltaPitch);
        else
        {
            deltaYaw = _delta.x * sensitivity * (float)deltaTime / 1000f;
            deltaPitch = -_delta.y * sensitivity * (float)deltaTime / 1000f;
        }
        

        // 1. ī�޶� ȸ��
        yaw += (float)deltaYaw;
        pitch += (float)deltaPitch;
        yaw = Mathf.Clamp(yaw, -60f, 60f); // �� ȸ�� ����
        pitch = Mathf.Clamp(pitch, -60f, 60f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 2. ��� ���� ���
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

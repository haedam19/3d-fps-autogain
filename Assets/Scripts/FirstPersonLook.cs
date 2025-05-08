using UnityEngine;
using UnityEngine.InputSystem;

public class FirsetPersonLook : MonoBehaviour
{
    [SerializeField] private float sensitivity = 100f;

    private Mouse mouse;
    private float pitch = 0f;
    private float yaw = 0f;

    private void Awake(){ mouse = Mouse.current; }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        Vector2 delta = mouse.delta.ReadValue();
        Vector2 pos = mouse.position.ReadValue();
        bool clickFlag = mouse.press.wasPressedThisFrame;

        // 마우스 움직임 이벤트 발생
        if(delta.sqrMagnitude > 0)
        {
            int currentCond = GameManager3D.Instance.session.condIdx;
            GameManager3D.Instance.session._conditions[currentCond].AddMove(pos);
        }
        // 클릭 -> 다음 trial로 이동
        if (clickFlag)
        {
            bool hit = Physics.Raycast(transform.position, transform.forward, 1e3f, LayerMask.NameToLayer("Target"));
            GameManager3D.Instance.Click(hit);

        }
        yaw += delta.x;
        pitch -= delta.y;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
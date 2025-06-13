using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity Input System을 이용한 1인칭 카메라 회전 스크립트입니다.
/// 별도의 플레이어 바디 없이 카메라 자체를 상하좌우 회전시킵니다.
/// </summary>
public class SimpleFirstPersonCamera : MonoBehaviour
{
    [Tooltip("마우스 민감도 (높을수록 빠르게 회전)")]
    public float sensitivity = 100f;

    // 피치(X축)와 요(Y축) 회전 저장
    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        // 마우스 커서 숨기고 고정
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Unity Input System에서 마우스 델타 값 읽기
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // 감도 및 시간 보정 적용
        float mouseX = mouseDelta.x * sensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * sensitivity * Time.deltaTime;

        // Pitch: 위로 올릴수록 xRotation 감소, -80~80도로 제한
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Yaw: -80~80도로 제한
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -80f, 80f);

        // 카메라 회전 적용
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}

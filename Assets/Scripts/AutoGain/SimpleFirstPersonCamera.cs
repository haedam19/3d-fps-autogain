using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity Input System�� �̿��� 1��Ī ī�޶� ȸ�� ��ũ��Ʈ�Դϴ�.
/// ������ �÷��̾� �ٵ� ���� ī�޶� ��ü�� �����¿� ȸ����ŵ�ϴ�.
/// </summary>
public class SimpleFirstPersonCamera : MonoBehaviour
{
    [Tooltip("���콺 �ΰ��� (�������� ������ ȸ��)")]
    public float sensitivity = 100f;

    // ��ġ(X��)�� ��(Y��) ȸ�� ����
    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        // ���콺 Ŀ�� ����� ����
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Unity Input System���� ���콺 ��Ÿ �� �б�
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // ���� �� �ð� ���� ����
        float mouseX = mouseDelta.x * sensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * sensitivity * Time.deltaTime;

        // Pitch: ���� �ø����� xRotation ����, -80~80���� ����
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Yaw: -80~80���� ����
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -80f, 80f);

        // ī�޶� ȸ�� ����
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}

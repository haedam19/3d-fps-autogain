using UnityEngine;

public class ScreenProjectionTester : MonoBehaviour
{
    [Tooltip("�׽�Ʈ�� ī�޶� (������ MainCamera)")]
    public Camera cam;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        // 1) d ���
        float d = Screen.height / (2f * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2f));
        Debug.Log($"[ProjectionTest] computed depth d = {d}");

        // 2) ������ r �� 10��
        float[] rValues = new float[] { 0.5f, 1f, 2f, 5f, 10f, 20f, 50f, 100f, 200f, 500f };
        foreach (float r in rValues)
        {
            Vector3 worldA = new Vector3(-r / 2f, -r / 2f, d);
            Vector3 worldB = new Vector3(r / 2f, r / 2f, d);

            Vector3 screenA = cam.WorldToScreenPoint(worldA);
            Vector3 screenB = cam.WorldToScreenPoint(worldB);

            float pixelSize = screenB.x - screenA.x;
            Debug.Log($"[r={r}] world points {worldA:F2}��{worldB:F2} �� pixelSize = {pixelSize:F2}");
        }

        // 3) ȭ�� �� �𼭸��� �ش��ϴ� ��ǥ ����
        Vector3[] screenCorners = new Vector3[]
        {
            new Vector3(0,             0,              d),  // Bottom-Left
            new Vector3(0,             Screen.height,  d),  // Top-Left
            new Vector3(Screen.width,  0,              d),  // Bottom-Right
            new Vector3(Screen.width,  Screen.height,  d)   // Top-Right
        };
        string[] cornerNames = new string[] { "BL", "TL", "BR", "TR" };

        for (int i = 0; i < screenCorners.Length; i++)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(screenCorners[i]);
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            Debug.Log($"[Corner {cornerNames[i]}] screenIn = {screenCorners[i]} �� world = {worldPos:F2} �� screenOut = {screenPos:F2}");
        }
    }
}

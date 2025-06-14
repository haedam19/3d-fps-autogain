using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioController : MonoBehaviour
{
    // ��ǥ ���� ���� (16:9)
    public float targetAspect = 16f / 9f;

    void Start()
    {
        Camera cam = GetComponent<Camera>();

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = cam.rect;

        if (scaleHeight < 1.0f)
        {
            // ȭ���� �ʹ� ���� ���� �� �� ��/�Ʒ� ���� (letterbox)
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // ȭ���� ���� �� �� ��/�� ���� (pillarbox)
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
    }
}

using UnityEngine;
using UnityEngine.Experimental.AI;

public class AGTargetGenerator : MonoBehaviour
{
    public GameObject targetPrefab;
    public GameObject targetObj;

    [Tooltip("테스트할 카메라 (비어 있으면 MainCamera 사용)")]
    public Camera cam;

    [Header("실험 파라미터")]
    public float minApx; // 픽셀 단위 Ampiltude(거리) 최솟값
    public float maxApx; // 픽셀 단위 Ampiltude(거리) 최댓값
    public float minWpx; // 픽셀 단위 최소 지름
    public float maxWpx; // 픽셀 단위 최대 지름
    public float idThreshold = 0.1f;
    float minID, maxID;

    [Header("픽셀 ↔ 월드 단위 매핑")]
    [Tooltip("1 world-unit이 화면에서 몇 픽셀에 대응할지")]
    public float pixelsPerUnit = 10f;

    private float depthD; // 카메라에서 타겟을 배치할 깊이 (world-unit)
    Vector2 center; // 화면 중심 좌표

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        depthD = Screen.height / (2f * pixelsPerUnit * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2f));
        center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        minID = Mathf.Log(1f + minApx / maxWpx, 2f);
        maxID = Mathf.Log(1f + maxApx / minWpx, 2f);

        // 첫 타겟 생성
        GenerateNextTarget();
    }

    public void GenerateNextTarget()
    {
        if (targetObj != null)
            Destroy(targetObj);

        targetObj = Instantiate(targetPrefab, transform);
        targetObj.GetComponent<Target3D>().TargetOn();

        // 1) 목표 ID
        float ID = Random.Range(minID, maxID);

        float Wc, xc, yc, IDc;
        int safety = 0;
        do
        {
            Wc = Random.Range(minWpx, maxWpx);

            // — 거리·방향 별도 샘플링
            float dist = Random.Range(minApx, maxApx);
            float angle = Random.Range(0f, 2f * Mathf.PI);
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            // 화면 좌표
            xc = Mathf.Clamp(center.x + offset.x, 0f, Screen.width);
            yc = Mathf.Clamp(center.y + offset.y, 0f, Screen.height);

            IDc = Mathf.Log(1f + dist / Wc, 2f);
            if (++safety > 1000) break;
        }
        while (Mathf.Abs(IDc - ID) >= idThreshold);

        // 스케일 & 위치 적용
        float worldDiameter = Wc / pixelsPerUnit;
        targetObj.transform.localScale = Vector3.one * worldDiameter;

        Vector3 screenPos = new Vector3(xc, yc, depthD);
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        // worldPos = worldPos.normalized * depthD; // 카메라에서 일정 깊이로 위치 조정
        targetObj.transform.position = worldPos;
    }

    void Update()
    {
        // 마우스 클릭 시 다음 타겟 생성 (실험용)
        if (Input.GetMouseButtonDown(0))
        {
            GenerateNextTarget();
        }
    }
}

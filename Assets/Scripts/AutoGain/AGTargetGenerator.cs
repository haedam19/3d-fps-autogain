using UnityEngine;
using UnityEngine.Experimental.AI;

public class AGTargetGenerator : MonoBehaviour
{
    public GameObject targetPrefab;
    public GameObject targetObj;

    [Tooltip("테스트할 카메라 (비어 있으면 MainCamera 사용)")]
    public Camera cam;
    public AGMouse cameraController;

    [Header("실험 파라미터")]
    public float minApx; // 픽셀 단위 Ampiltude(거리) 최솟값
    public float maxApx; // 픽셀 단위 Ampiltude(거리) 최댓값
    public float minWpx; // 픽셀 단위 최소 지름
    public float maxWpx; // 픽셀 단위 최대 지름

    [Header("픽셀 ↔ 월드 단위 매핑")]
    [Tooltip("1 world-unit이 화면에서 몇 픽셀에 대응할지")]
    public float pixelsPerUnit = 10f;

    private float depthD; // 카메라에서 타겟을 배치할 깊이 (world-unit)
    Vector2 center; // 화면 중심 좌표

    [Header("화면 여백 설정")]
    public int margin_h; // 상하 여백 픽셀
    public int margin_w; // 좌우 여백 픽셀
    Vector2 worldBottomLeft; // 월드좌표계 내 타겟 생성 영역 제한용
    Vector2 worldTopRight; // 월드좌표계 내 타겟 생성 영역 제한용

    [Header("Target 위치")]
    public Vector3 targetPos;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        depthD = Screen.height / (2f * pixelsPerUnit * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2f));
        center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        
        Vector3 screenBottomLeft = new Vector3(margin_w, margin_h, depthD); // 왼쪽 아래 모서리
        Vector3 screenTopright = new Vector3(Screen.width - margin_w, Screen.height - margin_h, depthD); // 오른쪽 위 모서리
        cameraController.ResetCameraRotation(); // 카메라 회전 초기화
        worldBottomLeft = cam.ScreenToWorldPoint(screenBottomLeft);
        worldTopRight = cam.ScreenToWorldPoint(screenTopright);

    }

    public AGTargetData GenerateNextTarget()
    {
        if (targetObj != null)
            Destroy(targetObj);

        targetObj = Instantiate(targetPrefab, transform);
        // targetObj.GetComponent<Target3D>().TargetOn();

        float xc, yc, wc;
        Vector3 worldPos, currentScreenPos;

        wc = Random.Range(minWpx, maxWpx);

        bool isValid;
        int safety = 0;
        do
        {
            xc = Random.Range(worldBottomLeft.x, worldTopRight.x);
            yc = Random.Range(worldBottomLeft.y, worldTopRight.y);
            worldPos = new Vector3(xc, yc, depthD);
            currentScreenPos = cam.WorldToScreenPoint(worldPos); // 현재 카메라 스크린 좌표로 변환

            float dist = Vector2.Distance(currentScreenPos, center);
            isValid = dist >= minApx && dist <= maxApx;

            if(++safety > 1000)
            {
                Debug.LogWarning("타겟 생성 실패: 많은 시도 반복 후에도 정상적인 타겟 생성 불가");
                // 이 경우 타겟 생성 평면 밖으로 카메라가 벗어났을 확률이 크므로 카메라 회전값 초기화 후 재시도할 것
                // cameraController.ResetCameraRotation(); 
                return AGTargetData.Empty;
            }
        } while (!isValid);

        // 타겟 width 조정
        // 1. 카메라와 타겟 사이의 거리를 depthD로 설정
        // 2. 타겟의 scale을 worldDiameter로 설정
        worldPos = worldPos.normalized * depthD;
        targetObj.transform.position = worldPos;
        float worldDiameter = wc / pixelsPerUnit;
        targetObj.transform.localScale = Vector3.one * worldDiameter;

        
        AGTarget agTarget = targetObj.GetComponent<AGTarget>();
        agTarget.RecordTargetData(wc);
        AGTargetData targetData = agTarget.data;

        return targetData;
    }

    void Update()
    {
        // 유니티 에디터에서 타겟 위치 확인용
        if(targetObj != null)
        {
            targetPos = targetObj.transform.position;
        }
    }
}

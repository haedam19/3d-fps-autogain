using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MouseLog;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Xml;

/// <summary>
/// 3D 타겟 실험의 타겟 및 Condition 오브젝트 풀/전환을 관리합니다.
/// </summary>
public class TargetManager3D : MonoBehaviour
{
    [SerializeField] GameObject targetPrefab;
    [SerializeField] GameObject objectPoolRoot;

    // 각 Condition_{index} 오브젝트를 인덱스 순서대로 관리
    private List<GameObject> conditionRoots = new List<GameObject>();

    // 현재 실험장에 전개된 Condition 오브젝트 인덱스
    private int currentConditionIndex = -1;

    // 카메라 기준 Z 위치
    private float distanceToCamera;

    public void Init()
    {
        // 카메라 기준 Z 위치 계산
        distanceToCamera = 0.1f * Screen.height / (2 * Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad / 2));
        transform.position = new Vector3(0f, 0f, distanceToCamera);
    }

    /// <summary>
    /// ConditionData에서 호출. Condition별 타겟 그룹을 생성하고 objectPoolRoot로 이동시킴.
    /// </summary>
    /// <returns>생성된 Target3D 리스트</returns>
    public List<Target3D> CreateTargets(int conditionIndex, int targetCount, ConditionConfig condition)
    {
        // 이미 생성된 Condition_{conditionIndex}가 있으면 삭제
        if (conditionIndex < conditionRoots.Count && conditionRoots[conditionIndex] != null)
        {
            Destroy(conditionRoots[conditionIndex]);
            conditionRoots[conditionIndex] = null;
        }

        // Condition_{conditionIndex} 빈 오브젝트 생성 (부모: this)
        GameObject conditionRoot = new GameObject($"Condition_{conditionIndex}");
        conditionRoot.transform.SetParent(transform, false);
        // 명시적으로 상대좌표 (0, 0, 0)으로 설정. 월드 좌표는 (0, 0, cameraToDistance)로 설정됨.
        conditionRoot.transform.localPosition = Vector3.zero; 

        // 타겟 생성 및 원형 배치 (+90도부터 시계 방향)
        List<Target3D> targets = new List<Target3D>(targetCount);
        float radius = condition.A / 2f;
        for (int i = 0; i < targetCount; i++)
        {
            double rad = Math.PI / 2.0 - (2.0 * Math.PI / targetCount) * i;
            double x = 0.1 * radius * Math.Cos(rad);
            double y = 0.1 * radius * Math.Sin(rad);

            GameObject targetObj = Instantiate(targetPrefab, conditionRoot.transform);
            targetObj.transform.localPosition = new Vector3((float)x, (float)y, 0f);
            targetObj.transform.position = targetObj.transform.position.normalized * distanceToCamera;

            Target3D t = targetObj.GetComponent<Target3D>();
            t.Radius = condition.W;
            t.CenterV = new Vector2((float)(x + Screen.width / 2.0), (float)(y + Screen.height / 2.0));

            targets.Add(t);
        }

        // objectPoolRoot로 부모 변경
        conditionRoot.transform.SetParent(objectPoolRoot.transform, false);

        // conditionRoots 리스트에 등록
        while (conditionRoots.Count <= conditionIndex)
            conditionRoots.Add(null);
        conditionRoots[conditionIndex] = conditionRoot;

        return targets;
    }

    /// <summary>
    /// Condition 전환: 이전 Condition 오브젝트는 objectPoolRoot로, 새 Condition 오브젝트는 실험장(this)으로 이동
    /// </summary>
    /// <param name="nextConditionIndex">전개할 Condition 인덱스</param>
    public void SetActiveCondition(int nextConditionIndex)
    {
        // 이전 Condition 오브젝트를 objectPoolRoot로 이동
        if (currentConditionIndex >= 0 && currentConditionIndex < conditionRoots.Count)
        {
            GameObject prevRoot = conditionRoots[currentConditionIndex];
            if (prevRoot != null)
                prevRoot.transform.SetParent(objectPoolRoot.transform, false);
        }

        // 새 Condition 오브젝트를 실험장(this)으로 이동
        if (nextConditionIndex >= 0 && nextConditionIndex < conditionRoots.Count)
        {
            GameObject nextRoot = conditionRoots[nextConditionIndex];
            if (nextRoot != null)
            {
                nextRoot.transform.SetParent(transform, false);
                nextRoot.transform.localPosition = Vector3.zero;
            }
        }

        currentConditionIndex = nextConditionIndex;
    }

    /// <summary>
    /// 모든 Condition 오브젝트와 타겟을 objectPoolRoot로 이동(초기화)
    /// </summary>
    public void ResetAllConditions()
    {
        for (int i = 0; i < conditionRoots.Count; i++)
        {
            if (conditionRoots[i] != null)
                conditionRoots[i].transform.SetParent(objectPoolRoot != null ? objectPoolRoot.transform : null, true);
        }
        currentConditionIndex = -1;
    }
}


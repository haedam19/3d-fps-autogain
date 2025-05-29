using MouseLog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target3D : MonoBehaviour
{
    // Target3D는 실험에서 사용되는 타겟 오브젝트에 부착되는 C# 컴포넌트입니다.
    //
    // 실험에서 ConditionData가 생성자에 의해 초기화될 때 해당 Condition을 위한 타겟 오브젝트들이 생성되고,
    // 각 Trial이 자신을 위한 Target3D 스크립트를 멤버변수에 저장해 참조하게 됩니다.
    // (한 Target3D가 여러 Trial에 사용될 수 있습니다.)
    //
    // 타겟이 생성된 직후 Target3D의 TargetOff()를 호출하여 비활성화되며,
    // 타겟 오브젝트는 ObjectPoolRoot의 자식으로 배치되어 해당 타겟이 실험에 쓰이는 condition차례가 될 때까지
    // 플레이어에게 보이지 않는 공간에서 대기합니다.
    // 
    // Target3D는 자기 차례가 되어 세팅될 때
    // Root의 WorldPosition이 (0, 0, distanceToCamera)이라는 가정 하에 position이 결정됩니다.
    // distanceToCamera는 카메라의 FOV와 화면 크기에 따라 결정됩니다.

    #region Fields
    ConditionData _cdata = null; // 속한 condition.
    TrialData _tdata = null; // 자신이 타겟이 되는 trial. 몇 번째 trial의 타겟인지 확인하는 데에 사용. 

    [SerializeField] Material m_onMaterial;
    [SerializeField] Material m_offMaterial;
    MeshRenderer m_renderer;
    SphereCollider m_collider;

    float radius;
    Vector2 posUnityScreen; // 좌하단 원점 좌표계 기준 위치
    PointR posDisplay;  // 좌상단 원점 좌표계 기준 위치
    #endregion

    #region Properties: Radius, Center
    public float Radius
    {
        get { return radius; }
        set
        {
            radius = value;
            transform.localScale = new Vector3(0.1f * value, 0.1f * value, 0.1f * value);
        }
    }

    public Vector2 CenterV
    {
        get { return posUnityScreen; }
        set
        {
            posUnityScreen = value;
            posDisplay = (PointR)value;
        }
    }

    public PointR CenterP {
        get { return posDisplay; }
        set
        {
            posDisplay = value;
            posUnityScreen = (Vector2)value;
        }
    }
    #endregion

    void Awake()
    {
        m_renderer = GetComponent<MeshRenderer>();
        m_collider = GetComponent<SphereCollider>();
        TargetOff();
    }

    public void TargetOn()
    {
        m_renderer.material = m_onMaterial;
        m_collider.enabled = true;
    }

    public void TargetOff()
    {
        m_renderer.material = m_offMaterial;
        m_collider.enabled = false;
    }

    public bool Contains(PointR p)
    {
        return PointR.Distance(CenterP, p) <= radius;
    }

    public void BindCondition(ConditionData cdata) { _cdata = cdata; }
    public void BindTrial(TrialData tdata) { _tdata = tdata; }
}

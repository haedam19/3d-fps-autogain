using MouseLog;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target3D : MonoBehaviour
{
    // Target3D�� ���迡�� ���Ǵ� Ÿ�� ������Ʈ�� �����Ǵ� C# ������Ʈ�Դϴ�.
    //
    // ���迡�� ConditionData�� �����ڿ� ���� �ʱ�ȭ�� �� �ش� Condition�� ���� Ÿ�� ������Ʈ���� �����ǰ�,
    // �� Trial�� �ڽ��� ���� Target3D ��ũ��Ʈ�� ��������� ������ �����ϰ� �˴ϴ�.
    // (�� Target3D�� ���� Trial�� ���� �� �ֽ��ϴ�.)
    //
    // Ÿ���� ������ ���� Target3D�� TargetOff()�� ȣ���Ͽ� ��Ȱ��ȭ�Ǹ�,
    // Ÿ�� ������Ʈ�� ObjectPoolRoot�� �ڽ����� ��ġ�Ǿ� �ش� Ÿ���� ���迡 ���̴� condition���ʰ� �� ������
    // �÷��̾�� ������ �ʴ� �������� ����մϴ�.
    // 
    // Target3D�� �ڱ� ���ʰ� �Ǿ� ���õ� ��
    // Root�� WorldPosition�� (0, 0, distanceToCamera)�̶�� ���� �Ͽ� position�� �����˴ϴ�.
    // distanceToCamera�� ī�޶��� FOV�� ȭ�� ũ�⿡ ���� �����˴ϴ�.

    #region Fields
    ConditionData _cdata = null; // ���� condition.
    TrialData _tdata = null; // �ڽ��� Ÿ���� �Ǵ� trial. �� ��° trial�� Ÿ������ Ȯ���ϴ� ���� ���. 

    [SerializeField] Material m_onMaterial;
    [SerializeField] Material m_offMaterial;
    MeshRenderer m_renderer;
    SphereCollider m_collider;

    float radius;
    Vector2 posUnityScreen; // ���ϴ� ���� ��ǥ�� ���� ��ġ
    PointR posDisplay;  // �»�� ���� ��ǥ�� ���� ��ġ
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

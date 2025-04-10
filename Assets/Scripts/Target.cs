using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] Material m_onMaterial;
    [SerializeField] Material m_offMaterial;
    MeshRenderer m_renderer;
    SphereCollider m_collider;
    float radius;
    public float Radius
    {
        get { return radius; }
        set { radius = value; transform.localScale = new Vector3(value, value, value); }
    }
    public Vector2 posOnScreen;

    void Awake()
    {
        m_renderer = GetComponent<MeshRenderer>();
        m_collider = GetComponent<SphereCollider>();
        TargetOff();
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

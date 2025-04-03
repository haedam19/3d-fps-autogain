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

    void Awake()
    {
        m_renderer = GetComponent<MeshRenderer>();
        m_collider = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

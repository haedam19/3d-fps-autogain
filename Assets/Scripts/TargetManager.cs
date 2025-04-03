using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    Vector2 prevMousePos;
    Target[] target;

    // Start is called before the first frame update
    void Start()
    {
        Mouse.current.WarpCursorPosition(Vector2.zero);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

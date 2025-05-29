using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class CursorAndMouse : MonoBehaviour
{
    public Mouse mouse;
    public Vector2 lastPos;
    public Vector2 currentPos;

    // Start is called before the first frame update
    void Start()
    {
        mouse = Mouse.current;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 _delta = mouse.delta.ReadValue();
        Vector2 _pos = mouse.position.ReadValue();
        if(_delta.sqrMagnitude > 0 )
        {
            string log = string.Format("delta: ({0}, {1}), pos: ({2}, {3})"
            , _delta.x, _delta.y, _pos.x, _pos.y);
            Debug.Log(log);
        }
    }
}

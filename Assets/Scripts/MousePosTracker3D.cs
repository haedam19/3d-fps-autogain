using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosTracker3D : MonoBehaviour
{
    float timeStamp;
    ushort buttonFlag;
    Vector2 mousePos;

    Queue<MouseEvent> mouseLog;

    public void Init()
    {
        mouseLog = new Queue<MouseEvent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager2D.Instance.playing)
            return;

        // Collect Mouse Event Data
        timeStamp = Time.time - GameManager3D.Instance.startTime;
        if (Input.GetMouseButtonDown(0))
            buttonFlag = 1;
        else if (Input.GetMouseButtonUp(0))
            buttonFlag = 2; 
        else
            buttonFlag = 0;
        mousePos = Input.mousePosition;

        // Do not record mouse event if there is no mouse movement and no left click
        if (Input.GetAxisRaw("Mouse X") == 0f && Input.GetAxisRaw("Mouse Y") == 0f && buttonFlag == 0)
            return;

        mouseLog.Enqueue(new MouseEvent(GameManager3D.Instance.envManager.conditionIndex, GameManager3D.Instance.trialIndex
            , buttonFlag, mousePos.x, mousePos.y, timeStamp));

        // Move to Next Trial 
        if (buttonFlag == 1)
            GameManager3D.Instance.Click();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] TMP_Text displaySizeText;
    [SerializeField] TMP_Text mousePosText;

    void Awake()
    {
        if (!displaySizeText)
            displaySizeText = GameObject.Find("Display Size Text").GetComponent<TMP_Text>();
        if (!mousePosText)
            mousePosText = GameObject.Find("Mouse Pos Text").GetComponent<TMP_Text>();
    }

    void Start()
    {
        displaySizeText.text = string.Format("{0} x {1}", Screen.width, Screen.height);
        mousePosText.text = string.Format("({0}, {1})", (int)(Input.mousePosition.x), (int)(Input.mousePosition.y));
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Screen.height - Input.mousePosition.y;
        mousePosText.text = string.Format("({0}, {1})", (int)(Input.mousePosition.x), (int)(Input.mousePosition.y));
    }
}

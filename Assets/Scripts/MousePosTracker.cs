using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MousePosTracker : MonoBehaviour
{
    [SerializeField] TMP_Text displaySizeText;
    [SerializeField] TMP_Text mousePosText;

    private void Awake()
    {
        if (!displaySizeText)
            displaySizeText = GameObject.Find("Display Size Text").GetComponent<TMP_Text>();
        if (!mousePosText)
            mousePosText = GameObject.Find("Mouse Pos Text").GetComponent<TMP_Text>();
    }
    // Start is called before the first frame update
    void Start()
    {

        displaySizeText.text = string.Format("{0} x {1}", Screen.width, Screen.height);
        mousePosText.text = string.Format("({0}, {1})", (int)(Input.mousePosition.x), (int)(Input.mousePosition.y));
    }

    // Update is called once per frame
    void Update()
    {
        mousePosText.text = string.Format("({0}, {1})", (int)(Input.mousePosition.x), (int)(Input.mousePosition.y));
    }
}

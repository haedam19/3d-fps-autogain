using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 실험 도중 메세지 박스를 열고 닫는 동작을 담당합니다. 
/// 실험을 시작했을 때, 한 condition이 종료되었을 때, 실험이 종료되었을 때
/// 메세지박스에 필요한 내용을 담아 출력해줍니다.
/// </summary>
public class UIManager3D : MonoBehaviour
{
    #region Singleton Instance
    static UIManager3D instance;
    public static UIManager3D Instance
    {
        get
        {
            if (instance == null)
                return null;
            else
                return instance;
        }
    }
    #endregion

    private GameObject startMsgBox;

    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }

    public void Init()
    {

    }

    public void ShowSessionStartMsgBox(SessionConfiguration config)
    {
        // 이미 메시지 박스가 있으면 중복 생성 방지
        if (startMsgBox != null)
        {
            startMsgBox.SetActive(true);
            return;
        }

        // Canvas 찾기 또는 생성
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 메시지 박스 패널 생성
        startMsgBox = new GameObject("StartMsgBox");
        startMsgBox.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = startMsgBox.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600); // 2배로 확대
        Image panelImage = startMsgBox.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f);

        // 텍스트 생성
        GameObject textObj = new GameObject("MsgText");
        textObj.transform.SetParent(startMsgBox.transform, false);
        Text msgText = textObj.AddComponent<Text>();
        msgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgText.alignment = TextAnchor.UpperCenter;
        msgText.color = Color.white;
        msgText.fontSize = 32;
        msgText.rectTransform.anchoredPosition = new Vector2(0, 90); // Y값도 2배
        msgText.rectTransform.sizeDelta = new Vector2(760, 360); // 2배

        // SessionConfiguration 정보 표시
        msgText.text =
            $"Ready to start the experiment!\n\n" +
            $"Subject ID: {config.subject}\n" +
            $"Type: {(config.isCircular ? "Circular" : "Ribbon")}\n" +
            $"A (Amplitude): {string.Join(", ", config.a)}\n" +
            $"W (Width): {string.Join(", ", config.w)}\n" +
            $"Number of Trials: {config.trials}\n" +
            $"Number of Practice Trials: {config.practice}\n";

        // 시작 버튼 생성
        GameObject buttonObj = new GameObject("StartButton");
        buttonObj.transform.SetParent(startMsgBox.transform, false);
        Button startButton = buttonObj.AddComponent<Button>();
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.5f, 1f, 1f);
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(320, 80); // 2배
        btnRect.anchoredPosition = new Vector2(0, -200); // Y값도 2배

        // 버튼 텍스트
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = "Start";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.fontSize = 20;
        btnText.rectTransform.sizeDelta = btnRect.sizeDelta;

        // 버튼 클릭 이벤트 등록
        startButton.onClick.AddListener(() =>
        {
            startMsgBox.SetActive(false);
            GameManager3D.Instance.TestStart();
        });
    }

    public void ShowConditionEndMsgBox()
    {
        // Show a message box indicating the end of the condition
        Debug.Log("Condition has ended. Please proceed to the next step.");
    }

    public void ShowSessionEndMsgBox()
    {

    }

    public void OnNextConditionButtonClicked()
    {
        GameManager3D.Instance.NextCondition();
    }
}


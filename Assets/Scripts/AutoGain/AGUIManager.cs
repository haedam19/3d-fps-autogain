using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 독립변수 선택 UI를 담당하는 매니저 클래스입니다.
/// 두 모드 중 하나(Reference / AutoGain)를 선택하도록 사용자에게 안내합니다.
/// </summary>
public class AGUIManager : MonoBehaviour
{
    [SerializeField] TMP_Text lastTrialText;
    [SerializeField] TMP_Text trialCountText;

    private GameObject variableSelectBox;
    private GameObject stopMsgBox;
    private GameObject conditionEndMsgBox;

    public void ShowIndependentVariableSelectionUI()
    {
        if (variableSelectBox != null)
        {
            variableSelectBox.SetActive(true);
            return;
        }

        // 캔버스 찾기 또는 생성
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
        variableSelectBox = new GameObject("VariableSelectBox");
        variableSelectBox.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = variableSelectBox.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600);
        Image panelImage = variableSelectBox.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f);

        // 설명 텍스트
        GameObject textObj = new GameObject("MsgText");
        textObj.transform.SetParent(variableSelectBox.transform, false);
        Text msgText = textObj.AddComponent<Text>();
        msgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgText.alignment = TextAnchor.UpperCenter;
        msgText.color = Color.white;
        msgText.fontSize = 32;
        msgText.rectTransform.anchoredPosition = new Vector2(0, -90);
        msgText.rectTransform.sizeDelta = new Vector2(760, 360);
        msgText.text = "Please choose one of the following modes:";

        // Reference 버튼
        GameObject refBtnObj = new GameObject("ReferenceButton");
        refBtnObj.transform.SetParent(variableSelectBox.transform, false);
        Button refButton = refBtnObj.AddComponent<Button>();
        Image refBtnImage = refBtnObj.AddComponent<Image>();
        refBtnImage.color = new Color(0.3f, 0.6f, 1f, 1f);
        RectTransform refBtnRect = refBtnObj.GetComponent<RectTransform>();
        refBtnRect.sizeDelta = new Vector2(260, 180);
        refBtnRect.anchoredPosition = new Vector2(-150, -200);

        GameObject refBtnTextObj = new GameObject("ButtonText");
        refBtnTextObj.transform.SetParent(refBtnObj.transform, false);
        Text refBtnText = refBtnTextObj.AddComponent<Text>();
        refBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        refBtnText.text = "Reference";
        refBtnText.alignment = TextAnchor.MiddleCenter;
        refBtnText.color = Color.white;
        refBtnText.fontSize = 40;
        refBtnText.rectTransform.sizeDelta = refBtnRect.sizeDelta;

        // AutoGain 버튼
        GameObject agBtnObj = new GameObject("AutoGainButton");
        agBtnObj.transform.SetParent(variableSelectBox.transform, false);
        Button agButton = agBtnObj.AddComponent<Button>();
        Image agBtnImage = agBtnObj.AddComponent<Image>();
        agBtnImage.color = new Color(0.3f, 0.6f, 1f, 1f);
        RectTransform agBtnRect = agBtnObj.GetComponent<RectTransform>();
        agBtnRect.sizeDelta = new Vector2(260, 180);
        agBtnRect.anchoredPosition = new Vector2(150, -200);

        GameObject agBtnTextObj = new GameObject("ButtonText");
        agBtnTextObj.transform.SetParent(agBtnObj.transform, false);
        Text agBtnText = agBtnTextObj.AddComponent<Text>();
        agBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        agBtnText.text = "AutoGain";
        agBtnText.alignment = TextAnchor.MiddleCenter;
        agBtnText.color = Color.white;
        agBtnText.fontSize = 40;
        agBtnText.rectTransform.sizeDelta = agBtnRect.sizeDelta;

        // 버튼 이벤트
        refButton.onClick.AddListener(() =>
        {
            variableSelectBox.SetActive(false);
            AGManager.Instance.SetGainMode(AGManager.GainMode.REFERENCE);
        });

        agButton.onClick.AddListener(() =>
        {
            variableSelectBox.SetActive(false);
            AGManager.Instance.SetGainMode(AGManager.GainMode.AUTOGAIN);
        });
    }

    public void ShowStopMsgBox(bool interrupted)
    {
        string content;
        if (interrupted)
        {
            content = "The next target could not be positioned properly,\n" +
                "so the experiment has been interrupted.\n" +
                "Press the Continue button to resume.";
        }
        else
        {
            content = "Experiment has been paused.\nPress the Continue button to resume.";
        }

        // 이미 메시지 박스가 있으면 중복 생성 방지
        if (stopMsgBox != null)
        {
            stopMsgBox.SetActive(true);
            // 텍스트 갱신
            var msgText = stopMsgBox.transform.Find("MsgText")?.GetComponent<Text>();
            if (msgText != null)
            {
                msgText.text = content;
            }
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
        stopMsgBox = new GameObject("ExpStopMsgBox");
        stopMsgBox.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = stopMsgBox.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600);
        Image panelImage = stopMsgBox.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f);

        // 텍스트 생성
        GameObject textObj = new GameObject("MsgText");
        textObj.transform.SetParent(stopMsgBox.transform, false);
        Text msgTextComp = textObj.AddComponent<Text>();
        msgTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgTextComp.alignment = TextAnchor.MiddleCenter;
        msgTextComp.color = Color.white;
        msgTextComp.fontSize = 32;
        msgTextComp.rectTransform.anchoredPosition = new Vector2(0, 90);
        msgTextComp.rectTransform.sizeDelta = new Vector2(760, 360);
        msgTextComp.text = content;

        // Continue 버튼 생성
        GameObject buttonObj = new GameObject("ContinuetButton");
        buttonObj.transform.SetParent(stopMsgBox.transform, false);
        Button nextButton = buttonObj.AddComponent<Button>();
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.5f, 1f, 1f);
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(320, 80);
        btnRect.anchoredPosition = new Vector2(0, -200);

        // 버튼 텍스트
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = "Continue";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.fontSize = 20;
        btnText.rectTransform.sizeDelta = btnRect.sizeDelta;

        // 버튼 클릭 이벤트 등록
        nextButton.onClick.AddListener(() =>
        {
            stopMsgBox.SetActive(false);
            AGManager.Instance.StartTest();
        });
    }

    public void UpdateStatusHUD(int completeTrial, int maxTrial, AGTrialData lastTrial)
    {
        AGTargetData targetData = lastTrial.ThisTarget;
        double endX = lastTrial.End.X;
        double endY = lastTrial.End.Y;
        double targetPosX = targetData.posR.X;
        double targetPosY = targetData.posR.Y;
        bool IsError = lastTrial.IsError;
        
        string lastTrialInfo = $"Last Target: ({targetPosX:F1}, {targetPosY:F1})\nLast Click: ({endX:F1}, {endY:F1})";
        string trialCountInfo = $"Trials: {completeTrial}/{maxTrial}";

        Color nextColor = IsError ? Color.yellow : Color.white;
        lastTrialText.text = lastTrialInfo;
        lastTrialText.color = nextColor;
        trialCountText.text = trialCountInfo;
        trialCountText.color = nextColor;
    }

    public void ShowEndMsgBox()
    {
        // 중복 방지: 기존 메시지 박스 제거 또는 숨기기
        if (conditionEndMsgBox != null)
            conditionEndMsgBox.SetActive(false);

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
        GameObject sessionEndMsgBox = new GameObject("SessionEndMsgBox");
        sessionEndMsgBox.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = sessionEndMsgBox.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600);
        Image panelImage = sessionEndMsgBox.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f);

        // 텍스트 생성
        GameObject textObj = new GameObject("MsgText");
        textObj.transform.SetParent(sessionEndMsgBox.transform, false);
        Text msgTextComp = textObj.AddComponent<Text>();
        msgTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgTextComp.alignment = TextAnchor.MiddleCenter;
        msgTextComp.color = Color.white;
        msgTextComp.fontSize = 32;
        msgTextComp.rectTransform.anchoredPosition = new Vector2(0, 90);
        msgTextComp.rectTransform.sizeDelta = new Vector2(760, 360);
        msgTextComp.text = "Thank you for participating in the experiment.\nThe program will now exit.";

        // 종료 버튼 생성
        GameObject buttonObj = new GameObject("CloseButton");
        buttonObj.transform.SetParent(sessionEndMsgBox.transform, false);
        Button closeButton = buttonObj.AddComponent<Button>();
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.5f, 1f, 1f);
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(320, 80);
        btnRect.anchoredPosition = new Vector2(0, -200);

        // 버튼 텍스트
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = "Exit";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.fontSize = 20;
        btnText.rectTransform.sizeDelta = btnRect.sizeDelta;

        // 버튼 클릭 이벤트 등록
        closeButton.onClick.AddListener(() =>
        {
            sessionEndMsgBox.SetActive(false);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }
}

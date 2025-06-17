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
    private GameObject variableSelectBox;

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

    public void ShowSubmovementHUD(bool isCorrect)
    {
 
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ���� ���� �޼��� �ڽ��� ���� �ݴ� ������ ����մϴ�. 
/// ������ �������� ��, �� condition�� ����Ǿ��� ��, ������ ����Ǿ��� ��
/// �޼����ڽ��� �ʿ��� ������ ��� ������ݴϴ�.
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

    private GameObject startMsgBox; // �ڵ�� ����
    private GameObject conditionEndMsgBox; // ���� �����ΰ� �ν����� �Ҵ�

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
        // �̹� �޽��� �ڽ��� ������ �ߺ� ���� ����
        if (startMsgBox != null)
        {
            startMsgBox.SetActive(true);
            return;
        }

        // Canvas ã�� �Ǵ� ����
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // �޽��� �ڽ� �г� ����
        startMsgBox = new GameObject("StartMsgBox");
        startMsgBox.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = startMsgBox.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600); // 2��� Ȯ��
        Image panelImage = startMsgBox.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f);

        // �ؽ�Ʈ ����
        GameObject textObj = new GameObject("MsgText");
        textObj.transform.SetParent(startMsgBox.transform, false);
        Text msgText = textObj.AddComponent<Text>();
        msgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgText.alignment = TextAnchor.UpperCenter;
        msgText.color = Color.white;
        msgText.fontSize = 32;
        msgText.rectTransform.anchoredPosition = new Vector2(0, 90); // Y���� 2��
        msgText.rectTransform.sizeDelta = new Vector2(760, 360); // 2��

        // SessionConfiguration ���� ǥ��
        msgText.text =
            $"Ready to start the experiment!\n\n" +
            $"Subject ID: {config.subject}\n" +
            $"Type: {(config.isCircular ? "Circular" : "Ribbon")}\n" +
            $"A (Amplitude): {string.Join(", ", config.a)}\n" +
            $"W (Width): {string.Join(", ", config.w)}\n" +
            $"Number of Trials: {config.trials}\n" +
            $"Number of Practice Trials: {config.practice}\n";

        // ���� ��ư ����
        GameObject buttonObj = new GameObject("StartButton");
        buttonObj.transform.SetParent(startMsgBox.transform, false);
        Button startButton = buttonObj.AddComponent<Button>();
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.5f, 1f, 1f);
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(320, 80); // 2��
        btnRect.anchoredPosition = new Vector2(0, -200); // Y���� 2��

        // ��ư �ؽ�Ʈ
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = "Start";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.fontSize = 20;
        btnText.rectTransform.sizeDelta = btnRect.sizeDelta;

        // ��ư Ŭ�� �̺�Ʈ ���
        startButton.onClick.AddListener(() =>
        {
            startMsgBox.SetActive(false);
            GameManager3D.Instance.TestStart();
        });
    }

    public void ShowConditionEndMsgBox(int completeConditions, int totalConditions)
    {
        // �̹� �޽��� �ڽ��� ������ �ߺ� ���� ����
        if (conditionEndMsgBox != null)
        {
            conditionEndMsgBox.SetActive(true);
            // �ؽ�Ʈ ����
            var msgText = conditionEndMsgBox.transform.Find("MsgText")?.GetComponent<Text>();
            if (msgText != null)
            {
                msgText.text = $"{completeConditions} of {totalConditions} conditions complete.\nPlease proceed to the next condition.";
            }
            return;
        }

        // Canvas ã�� �Ǵ� ����
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // �޽��� �ڽ� �г� ����
        conditionEndMsgBox = new GameObject("ConditionEndMsgBox");
        conditionEndMsgBox.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = conditionEndMsgBox.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600);
        Image panelImage = conditionEndMsgBox.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.95f);

        // �ؽ�Ʈ ����
        GameObject textObj = new GameObject("MsgText");
        textObj.transform.SetParent(conditionEndMsgBox.transform, false);
        Text msgTextComp = textObj.AddComponent<Text>();
        msgTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        msgTextComp.alignment = TextAnchor.MiddleCenter;
        msgTextComp.color = Color.white;
        msgTextComp.fontSize = 32;
        msgTextComp.rectTransform.anchoredPosition = new Vector2(0, 90);
        msgTextComp.rectTransform.sizeDelta = new Vector2(760, 360);
        msgTextComp.text = $"{completeConditions} of {totalConditions} conditions complete.\nPlease proceed to the next condition.";

        // Next ��ư ����
        GameObject buttonObj = new GameObject("NextButton");
        buttonObj.transform.SetParent(conditionEndMsgBox.transform, false);
        Button nextButton = buttonObj.AddComponent<Button>();
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.5f, 1f, 1f);
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(320, 80);
        btnRect.anchoredPosition = new Vector2(0, -200);

        // ��ư �ؽ�Ʈ
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = "Next";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.fontSize = 20;
        btnText.rectTransform.sizeDelta = btnRect.sizeDelta;

        // ��ư Ŭ�� �̺�Ʈ ���
        nextButton.onClick.AddListener(() =>
        {
            conditionEndMsgBox.SetActive(false);
            GameManager3D.Instance.NextCondition();
        });
    }

    public void ShowSessionEndMsgBox()
    {
        Debug.Log("���� ����");
    }
}


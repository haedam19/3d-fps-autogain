using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyStartBtn : MonoBehaviour
{
    Button startBtn;

    private void Awake()
    {
        startBtn = GetComponent<Button>();
        startBtn.onClick.AddListener(OnStartButtonClick);
    }

    private void OnStartButtonClick()
    {
        SceneManager.LoadScene("3D Fitts Test");
    }

}

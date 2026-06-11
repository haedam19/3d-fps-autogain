using UnityEngine;
using UnityEngine.SceneManagement;

[Tooltip("버튼으로 씬 로드 기능을 호출하기 위한 래퍼 컴포넌트")]
public class SceneLoadButton : MonoBehaviour
{
    public void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

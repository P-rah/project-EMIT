using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitBTN : MonoBehaviour
{
    private void OnEnable()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.StopBGM();
    }

    public void StartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex + 1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

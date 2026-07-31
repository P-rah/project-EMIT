using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitBTN : MonoBehaviour
{
    private void OnEnable()
    {
        if(SoundManager.instance != null ) SoundManager.instance.StopBGM();
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

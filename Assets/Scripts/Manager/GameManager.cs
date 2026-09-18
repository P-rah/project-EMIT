using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, UI_Open }
    public GameState currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Cursor.visible = false;
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        /*
        switch (currentState)
        {
            case GameState.Playing:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case GameState.Paused:
            case GameState.UI_Open:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
        */
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

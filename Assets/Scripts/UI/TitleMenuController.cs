using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuController : MonoBehaviour
{

    public Button NewGameButton => newGameButton;
    public Button ContinueButton => continueButton;
    public Button SettingsButton => settingsButton;
    public Button QuitButton => quitButton;

    [Header("타이틀 화면 버튼들")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private void OnEnable() 
    {
        quitButton.interactable = SceneManager.GetActiveScene().name != SceneDatabase.Scenes.MainMenu;

        newGameButton.onClick.AddListener(SceneControlManager.Instance.LoadFirstStage);
        settingsButton.onClick.AddListener(SceneControlManager.Instance.LoadSettings);
        quitButton.onClick.AddListener(GameManager.Instance.QuitGame);   
    }
}

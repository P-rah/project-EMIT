using UnityEngine;
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
        newGameButton.onClick.AddListener(SceneControlManager.Instance.LoadFirstStage);
        quitButton.onClick.AddListener(GameManager.Instance.QuitGame);    
    }
}

using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup titleUICanvas;

    public Button NewGameButton => newGameButton;
    public Button ContinueButton => continueButton;
    public Button SettingsButton => settingsButton;
    public Button QuitButton => quitButton;

    [Header("타이틀 화면 버튼들")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private bool isRegistered;

    private void OnEnable()
    {
        if (titleUICanvas == null) titleUICanvas = UIManager.ResolveCanvasGroup(newGameButton);

        isRegistered = UIManager.Instance.OverlayCanvas(titleUICanvas, this, newGameButton);
        if (!isRegistered)
        {
            enabled = false;
            return;
        }

        newGameButton.onClick.AddListener(SceneControlManager.Instance.LoadFirstStage);
        settingsButton.onClick.AddListener(SceneControlManager.Instance.LoadSettings);
        quitButton.onClick.AddListener(GameManager.Instance.QuitGame);
    }

    private void OnDisable()
    {
        if (!isRegistered) return;
        isRegistered = false;

        if (SceneControlManager.Instance != null)
        {
            newGameButton.onClick.RemoveListener(SceneControlManager.Instance.LoadFirstStage);
            settingsButton.onClick.RemoveListener(SceneControlManager.Instance.LoadSettings);
        }
        if (GameManager.Instance != null)
        {
            quitButton.onClick.RemoveListener(GameManager.Instance.QuitGame);
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveCanvas(titleUICanvas, this);
        }
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveCanvas(titleUICanvas, this);
        }
    }
}

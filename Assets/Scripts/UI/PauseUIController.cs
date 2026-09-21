using UnityEngine;
using UnityEngine.UI;

public class PauseUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    public Button ResumeButton => resumeButton;
    public Button RestartButton => restartButton;
    public Button SettingsButton => settingsButton;
    public Button ToTitleButton => toTitleButton;

    [Header("일시정지 화면 버튼들")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button toTitleButton;

    private bool isRegistered;
    private bool pauseSessionActive;

    private void OnEnable()
    {
        if (canvasGroup == null) canvasGroup = UIManager.ResolveCanvasGroup(resumeButton);
        isRegistered = UIManager.Instance.OverlayCanvas(canvasGroup, this, resumeButton);
        if (!isRegistered)
        {
            enabled = false;
            return;
        }

        if (!pauseSessionActive)
        {
            TimeManager.Instance.Pause();
            SoundManager.Instance.PauseAllSFX();
            GlobalInputHandler.Instance?.SetGameplayInputEnabled(false);
            pauseSessionActive = true;
        }

        resumeButton.onClick.AddListener(SceneControlManager.Instance.UnloadPause);
        restartButton.onClick.AddListener(SceneControlManager.Instance.ReloadStage);
        settingsButton.onClick.AddListener(SceneControlManager.Instance.LoadSettings);
        toTitleButton.onClick.AddListener(SceneControlManager.Instance.LoadTitle);
    }

    private void OnDisable()
    {
        if (!isRegistered) return;
        isRegistered = false;

        if (SceneControlManager.Instance != null)
        {
            resumeButton.onClick.RemoveListener(SceneControlManager.Instance.UnloadPause);
            restartButton.onClick.RemoveListener(SceneControlManager.Instance.ReloadStage);
            settingsButton.onClick.RemoveListener(SceneControlManager.Instance.LoadSettings);
            toTitleButton.onClick.RemoveListener(SceneControlManager.Instance.LoadTitle);
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveCanvas(canvasGroup, this);
        }
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RemoveCanvas(canvasGroup, this);
        }

        if (pauseSessionActive)
        {
            TimeManager.Instance?.Resume();
            SoundManager.Instance.UnPauseAllSFX();
            GlobalInputHandler.Instance?.SetGameplayInputEnabled(true);
            pauseSessionActive = false;
        }
    }

}

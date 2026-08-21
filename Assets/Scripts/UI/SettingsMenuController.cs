using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    public Button ExitButton => exitButton;
    public Button RestartButton => restartButton;
    public Button QuitButton => quitButton;
    public Slider SfxSlider => sfxSlider;
    public Slider BgmSlider => bgmSlider;

    [Header("타이틀 화면 버튼들")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private bool isClosing;

    private void OnEnable() 
    {
        isClosing = false;

        exitButton.onClick.AddListener(CloseSettings);
        restartButton.onClick.AddListener(RestartStage);
        quitButton.onClick.AddListener(ReturnToTitle);

        sfxSlider.SetValueWithoutNotify(SoundManager.instance.sfxPlayer.volume);
        bgmSlider.SetValueWithoutNotify(SoundManager.instance.bgmPlayer.volume);

        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
        bgmSlider.onValueChanged.AddListener(SetBgmVolume);
    }

    private void OnDisable()
    {
        exitButton.onClick.RemoveListener(CloseSettings);
        restartButton.onClick.RemoveListener(RestartStage);
        quitButton.onClick.RemoveListener(ReturnToTitle);
        sfxSlider.onValueChanged.RemoveListener(SetSfxVolume);
        bgmSlider.onValueChanged.RemoveListener(SetBgmVolume);
    }

    private void SetSfxVolume(float volume)
    {
        SoundManager.instance.sfxPlayer.volume = volume;
    }

    private void SetBgmVolume(float volume)
    {
        SoundManager.instance.bgmPlayer.volume = volume;
    }

    private void CloseSettings()
    {
        if (!TryBeginClosing()) return;

        SceneControlManager sceneController = SceneControlManager.Instance;
        if (sceneController == null)
        {
            Debug.LogError("SceneControlManager가 없어 설정 화면을 닫을 수 없습니다.");
            ResetClosingState();
            return;
        }

        sceneController.UnloadSettings();
    }

    private void RestartStage()
    {
        SceneControlManager.Instance.ReloadStage();
    }   

    private void ReturnToTitle()
    {
        if (!TryBeginClosing()) return;

        SceneControlManager sceneController = SceneControlManager.Instance;
        if (sceneController == null)
        {
            Debug.LogError("SceneControlManager가 없어 타이틀 화면으로 이동할 수 없습니다.");
            ResetClosingState();
            return;
        }

        sceneController.LoadTitle();
    }

    private bool TryBeginClosing()
    {
        if (isClosing) return false;

        isClosing = true;
        SetButtonsInteractable(false);
        return true;
    }

    private void ResetClosingState()
    {
        isClosing = false;
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        exitButton.interactable = interactable;
        restartButton.interactable = interactable;
        quitButton.interactable = interactable;
    }

}

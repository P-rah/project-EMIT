using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public Button ExitButton => exitButton;
    public Button QuitButton => quitButton;
    public Slider SfxSlider => sfxSlider;
    public Slider BgmSlider => bgmSlider;

    [Header("설정 화면 버튼들")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider bgmSlider;

    private bool isRegistered;

    private void OnEnable()
    {
        if (canvasGroup == null) canvasGroup = UIManager.ResolveCanvasGroup(quitButton);
        isRegistered = UIManager.Instance.OverlayCanvas(canvasGroup, this, quitButton);
        if (!isRegistered)
        {
            enabled = false;
            return;
        }

        quitButton.onClick.AddListener(SceneControlManager.Instance.UnloadSettings);

        sfxSlider.SetValueWithoutNotify(SoundManager.Instance.SfxVolume);
        bgmSlider.SetValueWithoutNotify(SoundManager.Instance.BgmVolume);

        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSfxVolume);
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBgmVolume);
    }

    private void OnDisable()
    {
        if (!isRegistered) return;
        isRegistered = false;

        if (SceneControlManager.Instance != null)
        {
            quitButton.onClick.RemoveListener(SceneControlManager.Instance.UnloadSettings);
        }
        if (SoundManager.Instance != null)
        {
            sfxSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetSfxVolume);
            bgmSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetBgmVolume);
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
    }

}

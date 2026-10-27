using UnityEngine;
using UnityEngine.UI;

public class LanguageSettingBTN : MonoBehaviour
{
    [Header("0: 한국어, 1: 영어")]
    [SerializeField] private int localeIndex;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnLanguageButtonClicked);
    }

    private void OnLanguageButtonClicked()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.ChangeLanguage(localeIndex);
        }
    }
}
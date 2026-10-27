using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 한국어(kor) : 0
    // 영어(en) : 1

    public void ChangeLanguage(int localeIndex)
    {
        StartCoroutine(SetLocaleRoutine(localeIndex));
    }

    private IEnumerator SetLocaleRoutine(int localeIndex)
    {
        yield return LocalizationSettings.InitializationOperation;

        if (localeIndex >= 0 && localeIndex < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance;

    [Header("조명 설정")]
    private Light2D globalLight;
    private Coroutine blinkLightRoutine;
    private float blinkTimeRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterLight(Light2D newLight)
    {
        globalLight = newLight;
        Debug.Log("글로벌 라이트 연결성공");
    }

    public void SetLightInstantly(float intensity)
    {
        globalLight.intensity = intensity;
    }

    public void ChangeLightSmoothly(float targetIntensity, float duration)
    {
        StopAllCoroutines();
        blinkLightRoutine = null;
        blinkTimeRemaining = 0f;
        StartCoroutine(LightTransitionRoutine(targetIntensity, duration));
    }

    private IEnumerator LightTransitionRoutine(float targetIntensity, float duration)
    {
        float startIntensity = globalLight.intensity;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);

            yield return null;
        }
    }

    public void BlinkLight(float intensity, float duration)
    {
        SetLightInstantly(intensity);
        blinkTimeRemaining = Mathf.Max(blinkTimeRemaining, duration);
        if (blinkLightRoutine == null)
            blinkLightRoutine = StartCoroutine(BlinkLightRoutine());
    }

    private IEnumerator BlinkLightRoutine()
    {
        do
        {
            yield return null;
            if (!TimeManager.Instance.IsPaused)
                blinkTimeRemaining -= Time.unscaledDeltaTime;
        }
        while (blinkTimeRemaining > 0f || TimeManager.Instance.IsPaused);

        blinkTimeRemaining = 0f;
        blinkLightRoutine = null;
        SetLightInstantly(1f);
    }
}

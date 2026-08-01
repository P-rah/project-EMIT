using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance;

    [Header("조명 설정")]
    public Light2D globalLight;

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

    public void SetLightInstantly(float intensity)
    {
        globalLight.intensity = intensity;
    }

    public void ChangeLightSmoothly(float targetIntensity, float duration)
    {
        StopAllCoroutines();
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
        StartCoroutine(BlinkLightRoutine(intensity, duration));
    }

    private IEnumerator BlinkLightRoutine(float intensity, float duration)
    {
        SetLightInstantly(intensity);

        yield return new WaitForSecondsRealtime(duration);

        SetLightInstantly(1f);
    }
}

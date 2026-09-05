using System.Collections;
using UnityEngine;

public class EmitShaderController : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.StartListening("OnEmitStart", StartEffect);
        EventManager.StartListening("OnEmitAborted", AbortEffect);
        EventManager.StartListening("OnEmitFinished", EndEffect);
        EventManager.StartListening("OnPlayerHit", startHitVinette);
        //EventManager.StartListening<int>("OnEmitRecoveryCount", setEmitUI);
    }
    private void OnDisable()
    {
        EventManager.StopListening("OnEmitStart", StartEffect);
        EventManager.StopListening("OnEmitAborted", AbortEffect);
        EventManager.StopListening("OnEmitFinished", EndEffect);
        EventManager.StopListening("OnPlayerHit", startHitVinette);
        //EventManager.StopListening<float>("OnEmitRecoveryCount", setEmitUI);
    }

    [SerializeField]
    private float _distortionTime = 0.75f;
    private Material _material;
    private static int _shaderRad = Shader.PropertyToID("_ShaderRad");
    private static int _startPoint = Shader.PropertyToID("_StartPoint");
    public Transform startPoint;
    private Coroutine emitDistortionAction;

    private static int _vinettingIntensity = Shader.PropertyToID("_VinettingIntensity");
    private float maxVinetteIntense = 0.2f;
    private float vinetteTime = 1f;

    //private static int _emitableAlpha = Shader.PropertyToID("_EmitableAlpha");



    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    private void StartEffect()
    {
        emitDistortionAction = StartCoroutine(EmitDistortionAction(-0.1f, 2f));
    }
    private void EndEffect()
    {
        emitDistortionAction = StartCoroutine(EmitDistortionAction(2f, -0.1f));
    }

    private void AbortEffect()
    {
        cancelEffect();
        _material.SetFloat(_shaderRad, 0f);
    }

    private void cancelEffect()
    {
        if(emitDistortionAction != null)
        {
            StopCoroutine(emitDistortionAction);
            emitDistortionAction = null;
        }
    }

    IEnumerator EmitDistortionAction(float startPos, float endPos)
    {
        _material.SetFloat(_shaderRad, startPos);
        float lerpedAmount = 0f;

        float elapsedTime = 0f;

        while(elapsedTime < _distortionTime)
        {
            elapsedTime += Time.deltaTime;

            _material.SetVector(_startPoint, (Vector2)Camera.main.WorldToViewportPoint(startPoint.position));
            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / _distortionTime));
            _material.SetFloat(_shaderRad, lerpedAmount);

            yield return null;
        }
    }

    private void startHitVinette()
    {
        StartCoroutine(HitVinetteAction());
    }

    IEnumerator HitVinetteAction()
    {
        float timer = 0f;
        _material.SetFloat(_vinettingIntensity, maxVinetteIntense);

        while (timer < vinetteTime)
        {
            timer += Time.unscaledDeltaTime;

            float currentIntensity = Mathf.Lerp(maxVinetteIntense, 0f, timer / vinetteTime);

            _material.SetFloat(_vinettingIntensity, currentIntensity);

            yield return null;
        }
        _material.SetFloat(_vinettingIntensity, 0f);
    }

    //private void setEmitUI(float rate)
    //{
    //    if(rate >= 5) _material.SetFloat(_emitableAlpha, 1f);
    //    else _material.SetFloat(_emitableAlpha, rate * 0.1f);
    //}
}

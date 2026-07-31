using System.Collections;
using UnityEngine;

public class EmitShaderController : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.StartListening("OnEmitPressed", StartEffect);
        EventManager.StartListening("OnEmitAborted", AbortEffect);
        EventManager.StartListening("OnEmitFinished", EndEffect);
    }
    private void OnDisable()
    {
        EventManager.StopListening("OnEmitPressed", StartEffect);
        EventManager.StopListening("OnEmitAborted", AbortEffect);
        EventManager.StopListening("OnEmitFinished", EndEffect);
    }

    [SerializeField]
    private float _distortionTime = 0.75f;
    private Material _material;
    private static int _shaderRad = Shader.PropertyToID("_ShaderRad");
    private static int _startPoint = Shader.PropertyToID("_StartPoint");
    public Transform startPoint;
    private Coroutine emitDistortionAction;

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
}

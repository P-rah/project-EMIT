using System.Collections;
using UnityEngine;

public class EmitShaderController : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.StartListening("EmitStart", StartEffect);
        EventManager.StartListening("EmitEnd", EndEffect);
    }
    private void OnDisable()
    {
        EventManager.StopListening("EmitStart", StartEffect);
        EventManager.StopListening("EmitEnd", EndEffect);
    }

    [SerializeField]
    private float _distortionTime = 0.75f;
    private Material _material;
    private static int _shaderRad = Shader.PropertyToID("_ShaderRad");
    private static int _startPoint = Shader.PropertyToID("_StartPoint");
    private Coroutine _emitDistorionCoroutine;
    public Transform startPoint;

    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    public void StartEffect()
    {
        _emitDistorionCoroutine = StartCoroutine(EmitDistortionAction(-0.1f, 2f));
    }
    public void EndEffect()
    {
        _emitDistorionCoroutine = StartCoroutine(EmitDistortionAction(2f, -0.1f));
    }

    private IEnumerator EmitDistortionAction(float startPos, float endPos)
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

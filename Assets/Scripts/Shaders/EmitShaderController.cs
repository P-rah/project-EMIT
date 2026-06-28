using System.Collections;
using UnityEngine;

public class EmitShaderController : MonoBehaviour
{
    [SerializeField]
    private float _distortionTime = 0.75f;
    private Material _material;
    private static int _distanceFormCenter = Shader.PropertyToID("_DistanceFromCenter");
    private Coroutine _emitDistorionCoroutine;


    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    public void StartEffect()
    {
        _emitDistorionCoroutine = StartCoroutine(EmitDistortionAction(-0.1f, 1f));
    }
    public void EndEffect()
    {
        _emitDistorionCoroutine = StartCoroutine(EmitDistortionAction(1f, -0.1f));
    }

    private IEnumerator EmitDistortionAction(float startPos, float endPos)
    {
        _material.SetFloat(_distanceFormCenter, startPos);
        float lerpedAmount = 0f;

        float elapsedTime = 0f;

        while(elapsedTime < _distortionTime)
        {
            elapsedTime += Time.deltaTime;

            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / _distortionTime));
            _material.SetFloat(_distanceFormCenter, lerpedAmount);

            yield return null;
        }
    }
}

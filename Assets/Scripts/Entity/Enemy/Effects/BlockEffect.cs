using System.Collections;
using UnityEngine;

public class BlockEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    private const float oneFrameSec = 0.1f;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
        DisableEffect();
    }

    public void DisableEffect()
    {
        spriteRenderer.sprite = null;
    }

    public void EnableEffect()
    {
        StartCoroutine(EffectAnimationRoutine());
    }

    private IEnumerator EffectAnimationRoutine()
    {
        foreach (var sprite in sprites) {
            spriteRenderer.sprite = sprite;
            yield return new WaitForSeconds(oneFrameSec);
        }
        DisableEffect();
    }
}

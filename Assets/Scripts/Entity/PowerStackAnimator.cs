using System.Collections;
using UnityEngine;

public class PowerStackAnimator : MonoBehaviour
{
    private SpriteRenderer SpriteRenderer;
    [SerializeField] private Sprite[] stackSprite;
    private const float oneFrameSec = 0.1f;

    private void OnEnable()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        SpriteRenderer.enabled = false;
    }

    public void ShowPowerStack()
    {
        if (SpriteRenderer.enabled) return;
        SpriteRenderer.enabled = true;
        StartCoroutine(PowerStafckAnimRoutine());
    }

    public void HidePowerStack()
    {
        StopCoroutine(PowerStafckAnimRoutine());
        SpriteRenderer.enabled = false;
    }

    private IEnumerator PowerStafckAnimRoutine()
    {
        for (int i = 0; i< stackSprite.Length; i++)
        {
            SpriteRenderer.sprite = stackSprite[i];
            yield return TimeManager.Instance.WaitForGameplaySeconds(oneFrameSec);
        }
    }

    //private IEnumerator PowerStafckAnimFadeRoutine()
    //{
    //    for (int i = stackSprite.Length - 1; i >= 0 ; i--)
    //    {
    //        SpriteRenderer.sprite = stackSprite[i];
    //        yield return new WaitForSecondsRealtime(oneFrameSec);
    //    }
    //    SpriteRenderer.enabled = false;
    //}
}

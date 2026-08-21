using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIGaugeBarController : MonoBehaviour
{
    [Header("UI 연결")]
    public Image frontHealthBar;    //실제 체력
    public Image ghostHealthBar;    //가상 체력

    [Header("연출 세팅")]
    public float ghostDelay = 0.3f;
    public float ghostDuration = 0.5f;

    private float maxAmount;
    private float currentAmount;

    public void SetMaxAmount(float maxValue)
    {
        this.maxAmount = maxValue;
        currentAmount = maxAmount;

        frontHealthBar.fillAmount = 1f;
        ghostHealthBar.fillAmount = 1f;
    }

    public void UpdateBar(float amount)
    {
        currentAmount = Mathf.Clamp(amount, 0, maxAmount);

        float targetRatio = currentAmount / maxAmount;

        frontHealthBar.fillAmount = targetRatio;

        ghostHealthBar.DOKill();
        ghostHealthBar.DOFillAmount(targetRatio, ghostDuration)
                      .SetDelay(ghostDelay)
                      .SetEase(Ease.OutCubic);
    }
}
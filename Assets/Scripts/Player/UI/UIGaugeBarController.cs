using UnityEngine;
using UnityEngine.UI;

public class UIGaugeBarController : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image fillImage;

    [Header("옵션 설정")]
    [SerializeField] private bool useSmoothDrain = false;
    [SerializeField] private float drainSpeed = 10f;

    private float targetFillAmount = 1f;

    void Update()
    {
        if (useSmoothDrain && fillImage.fillAmount != targetFillAmount)
        {
            fillImage.fillAmount = Mathf.Lerp(
                fillImage.fillAmount,
                targetFillAmount,
                Time.deltaTime * drainSpeed
            );
        }
    }

    public void UpdateBar(float currentRate, float maxAmount)
    {
        float fillRatio = currentRate / maxAmount;

        if (useSmoothDrain)
        {
            targetFillAmount = fillRatio;
        }
        else
        {
            fillImage.fillAmount = fillRatio;
        }
    }
}

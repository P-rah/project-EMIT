using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("UI 설정")]
    public RectTransform healthBarTransform;
    public RectTransform emitBarTransform;

    public Image healthBarImage;
    public Image emitBarImage;

    private void Start()
    {
        if (EmitManager.Instance != null)
        {
            EmitManager.Instance.ConnectPlayerUIController(this);
        }
    }


    public void UpdateHealthBar()
    {
        float UIRatio = (float)Player.PlayerHealth.currentPlayerHealth / Player.PlayerHealth.playerHealthMax;

        healthBarImage.fillAmount = UIRatio;
    }

    public void UpdateEmitBar(float current, float max)
    {
        emitBarImage.fillAmount = current / max;
    }

    public void SetUIFlip()
    {
        Vector3 currentScale = healthBarTransform.localScale;
        currentScale.x *= -1f;
        healthBarTransform.localScale = currentScale;

        currentScale = emitBarTransform.localScale;
        currentScale.x *= -1f;
        emitBarTransform.localScale = currentScale;
    }
}

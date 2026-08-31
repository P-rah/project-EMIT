using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("UI 설정")]
    //public RectTransform healthBarTransform;
    //public RectTransform emitBarTransform;

    public Image healthBarImage;
    public Image emitBarImage;
    [SerializeField] private float emitMaxGlowIntensity;
    [SerializeField] private float emitMinGlowIntensity;
    [SerializeField] private float glowSpeed;

    private bool glowOn;

    private static int _HDR_Intensity = Shader.PropertyToID("_HDR_Intensity");

    private void Start()
    {
        if (EmitManager.Instance != null)
        {
            EmitManager.Instance.ConnectPlayerUIController(this);
        }
    }

    private void FixedUpdate()
    {
        if (!glowOn) return;

        float t = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f;

        float glowIntensity = Mathf.Lerp(emitMinGlowIntensity, emitMaxGlowIntensity, t);

        emitBarImage.material.SetFloat(_HDR_Intensity, glowIntensity);
    }

    public void UpdateHealthBar()
    {
        float UIRatio = (float)Player.PlayerHealth.currentPlayerHealth / Player.PlayerHealth.playerHealthMax;

        healthBarImage.fillAmount = UIRatio;
    }

    public void UpdateEmitBar(float current, float max)
    {
        emitBarImage.fillAmount = current / max;
        if (emitBarImage.fillAmount == 1) glowOn = true;
        else
        {
            glowOn = false;
            emitBarImage.material.SetFloat(_HDR_Intensity, 2f);
        }
    }



    //public void SetUIFlip()
    //{
    //    Vector3 currentScale = healthBarTransform.localScale;
    //    currentScale.x *= -1f;
    //    healthBarTransform.localScale = currentScale;

    //    currentScale = emitBarTransform.localScale;
    //    currentScale.x *= -1f;
    //    emitBarTransform.localScale = currentScale;
    //}
}

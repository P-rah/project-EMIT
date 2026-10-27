using DG.Tweening;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력")]
    [SerializeField] public int maxPlayerHealth = 3;
    [SerializeField] public int currentPlayerHealth;

    [Header("무적 시간")]
    [SerializeField] [Range(0f, 5f)] private float invulnerableTime = 1f;
    [SerializeField] private bool isInvulnerable = false;

    [Header("피격 이펙트")]
    private SpriteRenderer renderer;
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
    private MaterialPropertyBlock flashProperties;
    private float flashAmount;
    private Tween hitFlashTween;

    private void Start()
    {
        setPlayerHealth(maxPlayerHealth);
    }

    private void OnEnable()
    {
        EventManager.StartListening("OnPlayerHit", decreasePlayerHealth);
        renderer = GetComponent<SpriteRenderer>();
    }
    private void OnDisable()
    {
        EventManager.StopListening("OnPlayerHit", decreasePlayerHealth);
        hitFlashTween?.Kill();
    }

    public void setPlayerHealth(int playerHealth)
    {
        if (playerHealth > maxPlayerHealth) return;
        this.currentPlayerHealth = playerHealth;
        Player.UIController.UpdateHealthBar();
    }

    private void decreasePlayerHealth()
    {
        if (isInvulnerable) return;

        currentPlayerHealth -= 1;
        EventManager.TriggerEvent("OnPlayerDamaged");

        if (currentPlayerHealth <= 0)
        {
            PlayerDead();
        }
        else
        {
            // 체력이 0이 되지 않았을 때에만 피격 연출
            HitFlash();
        }

        Player.UIController.UpdateHealthBar();
    }

    // 피격 시 플레이어 스프라이트 깜빡임
    private void HitFlash()
    {
        hitFlashTween?.Kill();
        isInvulnerable = true;

        Sequence sequence = DOTween.Sequence();
        hitFlashTween = sequence;
        sequence.SetUpdate(false).SetAutoKill(true).SetLink(gameObject)
            .OnKill(() =>
            {
                hitFlashTween = null;
                isInvulnerable = false;
                SetFlashAmount(0f);
            });

        if (renderer && renderer.sharedMaterial && renderer.sharedMaterial.HasProperty(FlashAmountId))
        {
            flashProperties ??= new MaterialPropertyBlock();
            SetFlashAmount(1f);

            // 1.0 -> 0.8 -> 0.6 세 번의 피크 후, 최대 0.5초 안에 원래 색으로 복원한다.
            float flashDuration = Mathf.Min(0.5f, invulnerableTime);
            sequence.Append(DOTween.To(() => flashAmount, SetFlashAmount, 0f, flashDuration * 0.12f).SetEase(Ease.InOutSine))
                .Append(DOTween.To(() => flashAmount, SetFlashAmount, 1f, flashDuration * 0.08f).SetEase(Ease.InOutSine))
                .Append(DOTween.To(() => flashAmount, SetFlashAmount, 0f, flashDuration * 0.12f).SetEase(Ease.InOutSine))
                .Append(DOTween.To(() => flashAmount, SetFlashAmount, 1f, flashDuration * 0.08f).SetEase(Ease.InOutSine))
                .Append(DOTween.To(() => flashAmount, SetFlashAmount, 0f, flashDuration * 0.6f).SetEase(Ease.OutSine))
                .AppendInterval(invulnerableTime - flashDuration);
        }
        else
        {
            // 셰이더가 피격 연출을 지원하지 않아도 무적 시간은 동일하게 유지한다.
            sequence.AppendInterval(invulnerableTime);
        }
    }

    private void SetFlashAmount(float amount)
    {
        flashAmount = amount;
        if (!renderer || flashProperties == null) return;

        renderer.GetPropertyBlock(flashProperties);
        flashProperties.SetFloat(FlashAmountId, amount);
        renderer.SetPropertyBlock(flashProperties);
    }

    private void PlayerDead()
    {
        Player.State.IsDead = true;
        //Player.Movement.StopMove();
        Player.TempMovement.StopMovement();
        Debug.Log("Player Dead");
    }

    public void AnimEvent_DeadEmitActive()
    {
        EventManager.TriggerEvent("OnPlayerDieEmit");
    }

    public void AnimEvent_SceneReset()
    {
        EventManager.TriggerEvent("StopEmit");
        StageManager.Instance.RestartStage();
    }
}

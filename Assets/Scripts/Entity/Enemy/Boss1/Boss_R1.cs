using System.Collections.Generic;
using UnityEngine;

public class Boss_R1 : EnemyBase
{
    private BossAnimator _bossAnimator;
    public Boss1AttackStrategy CurrentStrategy { get; set; }

    [SerializeField] private BlockEffect blockEffect;
    [SerializeField] private AudioClip blockSound;

    [Header("보스 공격 패턴 리스트")]
    [SerializeField] private Boss1AttackStrategy[] availableAttacks;

    private Boss1AttackState bossAttackState;


    public override void Init()
    {
        base.Init();

        _bossAnimator = GetComponent<BossAnimator>();
        StateMachine = GetComponent<Boss1StateMachine>();

        bossAttackState = new Boss1AttackState(StateMachine);
    }

    public virtual void Start()
    {
        StageManager.Instance?.AddEnemy();
        ChangeState(IdleState, false);
        emitManager = EmitManager.Instance;
    }

    public virtual void Update()
    {
        UpdateInvincibilityTimer();
        if (emitManager.isEmitting) return;

        StateMachine.UpdateState();
        UpdateTargetDirection();
        CalculatePowerTimer();
        CalculateHitEffectTimer();
        CheckDie();
    }

    public virtual void FixedUpdate()
    {
        if (isRooted) return;
        StateMachine.Move();
    }

    // ==========================================
    // Enemy 고유 로직
    // ==========================================
    public void ChangeState(IState nextState, bool force)
    {
        StateMachine?.ChangeState(nextState, force);
    }

    internal override void NotifyStateEntered(IState state) => _bossAnimator?.HandleStateEntered(StateMachine, state, currentKnockBackForce, FacingDir);
    internal override void NotifyGrabStarted(float horizontalOffset) {}

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    public bool isMovingLeft() => rb.linearVelocityX * Vector2.left.magnitude > 0f;
    public bool IsCurrentState(IState state) => StateMachine.IsCurrentState(state);

    public void EnableCancel()
    {
        if (CurrentState is EnemyAttackState attackState)
            attackState.SetCancellable(true);
    }

    // ==========================================
    // 보스 전용 전략 패턴 관련
    // ==========================================

    public Boss1AttackStrategy GetRandomAttackStrategy()
    {
        if (availableAttacks == null || availableAttacks.Length == 0)
            return null;

        // 1. 현재 실행 가능한(CanExecute) 전략들만 수집
        List<Boss1AttackStrategy> validStrategies = new List<Boss1AttackStrategy>();
        foreach (var strategy in availableAttacks)
        {
            if (strategy != null && strategy.CanExecute(this))
            {
                validStrategies.Add(strategy);
            }
        }

        if (validStrategies.Count == 0) return null;

        // 2. 랜덤 하나 선택
        int randomIndex = Random.Range(0, validStrategies.Count);
        return validStrategies[randomIndex];
    }

    internal override void NotifyAttackWindupStarted()
    {
        string animName = CurrentStrategy != null ? CurrentStrategy.windupAnimName : null;
        _bossAnimator?.HandlePlayAnimation(animName);
    }

    internal override void NotifyAttackStrikeStarted()
    {
        string animName = CurrentStrategy != null ? CurrentStrategy.strikeAnimName : null;
        _bossAnimator?.HandlePlayAnimation(animName);
    }

    public void AnimEvent_PlayAttackEffects()
    {
        float cameraShakeIntensity = CurrentStrategy.cameraShakeIntensity;
        float cameraShakeDuration = CurrentStrategy.cameraShakeDuration;
        if (cameraShakeIntensity > 0)
        {
            CameraShake.Instance?.ShakeCamera(cameraShakeIntensity, cameraShakeDuration);
        }

        if (myAudioSource != null && attackSound != null)
        {
            myAudioSource.PlayOneShot(attackSound);
        }
    }

    // ==========================================
    // 추상 메서드 오버라이드
    // ==========================================
    public override void ReceiveAttack(AttackInfo attackInfo)
    {
        if (IsInvincible) return;
        invincibilityTimer = invincibilityDuration;

        if (isFrontAttacked(attackInfo.knockback.x) && !emitManager.isEmitting)
        {
            myAudioSource.PlayOneShot(blockSound);
            blockEffect.EnableEffect();
            return;
        }

        if (CurrentState != null && CurrentState.CanBeCancelled()) ReceiveKnockback(attackInfo, true);

        if (transitFlipPower) FlipPower(attackInfo.knockback.x);

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        ResetPowerTImer();
        CalculatePower(attackInfo);

        transitFlipPower = false;
    }

    public override void ReceiveDamage(AttackInfo attackInfo)
    {
        int damage = DamageCalculation<Boss_R1>.Calculate(attackInfo, this);
        if (dmgType == 2) ResetPower();
        currentHealth -= damage;
    }

    public override void ReceiveKnockback(AttackInfo attackInfo, bool force)
    {
        myAudioSource.PlayOneShot(attackHitSound);
    }

    public override void ReceiveGrab()
    {
    }

    private void CheckDie()
    {
        if (currentHealth <= 0 && !emitManager.isEmitting)
        {
            if (SoundManager.Instance != null && deathSound != null) SoundManager.Instance.PlaySFX(deathSound);
            StageManager.Instance?.EnemyDied();
            ChangeState(DeadState, true);
            gameObject.SetActive(false);
        }
    }

    private bool isFrontAttacked(float attackDir)
    {
        return attackDir * FacingDir.x < 0 ? true : false;
    }
}
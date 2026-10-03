using UnityEngine;

public class Enemy : EnemyBase
{
    private EnemyAnimator _enemyAnimator;
    public EnemyCollisionDetector enemyCollisionDetector;

    public EnemyStateMachine EnemyStateMachine => StateMachine as EnemyStateMachine;

    public override void Init()
    {
        base.Init();

        _enemyAnimator = GetComponent<EnemyAnimator>();
        StateMachine = GetComponent<EnemyStateMachine>();
        enemyCollisionDetector = GetComponent<EnemyCollisionDetector>();
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
        CalculateHitEffectTimer();
        if (emitManager != null && emitManager.isEmitting) return;

        StateMachine?.UpdateState();
        UpdateTargetDirection();
        CalculatePowerTimer();
        CheckDie();
    }

    public virtual void FixedUpdate()
    {
        if (isRooted) return;
        StateMachine?.Move();
    }

    // ==========================================
    // Enemy 고유 로직
    // ==========================================
    public void ChangeState(IState nextState, bool force)
    {
        StateMachine?.ChangeState(nextState, force);
    }

    internal override void NotifyStateEntered(IState state) => _enemyAnimator?.HandleStateEntered(StateMachine, state, currentKnockBackForce, FacingDir);
    internal override void NotifyAttackWindupStarted() => _enemyAnimator?.HandleAttackWindupStarted();
    internal override void NotifyAttackStrikeStarted() => _enemyAnimator?.HandleAttackStrikeStarted();
    internal override void NotifyGrabStarted(float horizontalOffset) => _enemyAnimator?.HandleGrabStarted(horizontalOffset, FacingDir);

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    public bool isMovingLeft() => rb.linearVelocityX * Vector2.left.magnitude > 0f;
    public bool IsCurrentState(IState state) => StateMachine != null && StateMachine.IsCurrentState(state);

    // ==========================================
    // 추상 메서드 오버라이드
    // ==========================================
    public override void ReceiveAttack(AttackInfo attackInfo)
    {
        if (IsInvincible) return;
        invincibilityTimer = invincibilityDuration;

        if (CurrentState != null && CurrentState.CanBeCancelled())
        {
            ReceiveKnockback(attackInfo, true);
        }
        else if (CurrentState != null)
        {
            ReceiveKnockback(attackInfo, true);
        }

        if (transitFlipPower) FlipPower(attackInfo.knockback.x);

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        ResetPowerTImer();
        CalculatePower(attackInfo);

        transitFlipPower = false;
    }

    public override void ReceiveDamage(AttackInfo attackInfo)
    {
        int damage = DamageCalculation<Enemy>.Calculate(attackInfo, this);
        if (dmgType == 2) ResetPower();
        currentHealth -= damage;
    }

    public override void ReceiveKnockback(AttackInfo attackInfo, bool force)
    {
        currentKnockBackForce = attackInfo.knockback;
        isCurrentHitGrab = attackInfo.attackIsGrab;

        if (!isCurrentHitGrab)
        {
            if (EnemyStateMachine != null)
            {
                if (CurrentState == EnemyStateMachine.GrabbedState)
                    transitFlipPower = true;

                StateMachine.ChangeState(EnemyStateMachine.KnockbackState, force);
            }
        }
        else
        {
            if (EnemyStateMachine != null)
            {
                StateMachine.ChangeState(EnemyStateMachine.GrabbedState, force);
            }
        }

        if (myAudioSource != null && attackHitSound != null)
        {
            myAudioSource.PlayOneShot(attackHitSound);
        }
    }

    public override void ReceiveGrab()
    {
        if (CurrentState is SharedGrabbedState grabState)
        {
            grabState.Grab();
        }
    }

    private void CheckDie()
    {
        if (currentHealth <= 0 && (emitManager == null || !emitManager.isEmitting))
        {
            if (SoundManager.Instance != null && deathSound != null) SoundManager.Instance.PlaySFX(deathSound);
            StageManager.Instance?.EnemyDied();

            if (EnemyStateMachine != null)
            {
                ChangeState(EnemyStateMachine.DeadState, true);
            }

            gameObject.SetActive(false);
        }
    }

    public override void StartOverlapDetect()
    {
        enemyCollisionDetector?.startDetect();
    }

    public override void StopOverlapDetect()
    {
        enemyCollisionDetector?.stopDetect();
    }
}
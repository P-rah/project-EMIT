using UnityEngine;

public class Enemy_R2 : EnemyBase
{   
    private EnemyAnimator _enemyAnimator;
    public EnemyCollisionDetector enemyCollisionDetector;

    public EnemyStateMachine EnemyStateMachine => StateMachine as EnemyStateMachine;

    [SerializeField] private BlockEffect blockEffect;
    [SerializeField] private AudioClip blockSound;

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
    public bool IsCurrentState(IState state) => StateMachine.IsCurrentState(state);

    public void EnableCancel()
    {
        if (CurrentState is EnemyAttackState attackState)
            attackState.SetCancellable(true);
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

        if(transitFlipPower) FlipPower(attackInfo.knockback.x);

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        ResetPowerTImer();
        CalculatePower(attackInfo);

        transitFlipPower = false;
    }

    public override void ReceiveDamage(AttackInfo attackInfo)
    {
        int damage = DamageCalculation<Enemy_R2>.Calculate(attackInfo, this);
        if (dmgType == 2) ResetPower();
        currentHealth -= damage;
    }

    public override void ReceiveKnockback(AttackInfo attackInfo, bool force)
    {
        currentKnockBackForce = attackInfo.knockback;
        isCurrentHitGrab = attackInfo.attackIsGrab;
        bool shouldForceChange = force || canChangeHitState;

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
        myAudioSource.PlayOneShot(attackHitSound);
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

    public override void StartOverlapDetect()
    {
        enemyCollisionDetector.startDetect();
    }

    public override void StopOverlapDetect()
    {
        enemyCollisionDetector.stopDetect();
    }
}
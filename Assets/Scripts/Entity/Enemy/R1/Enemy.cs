using UnityEngine;

public class Enemy : EnemyBase
{
    public EnemyStateMachine StateMachine { get; private set; }
    public StateBase<EnemyStateMachine> CurrentState => StateMachine?.currentState;
    public StateBase<EnemyStateMachine> IdleState => StateMachine?.IdleState;
    public StateBase<EnemyStateMachine> DeadState => StateMachine?.DeadState;

    private EnemyAnimator _enemyAnimator;

    public override void Init()
    {
        base.Init();

        _enemyAnimator = GetComponent<EnemyAnimator>();
        StateMachine = GetComponent<EnemyStateMachine>();
    }

    public virtual void Start()
    {
        StageManager.Instance?.AddEnemy();
        ChangeState(IdleState, false);
    }

    public virtual void Update()
    {
        if (EmitManager.Instance.isEmitting) return;

        StateMachine.UpdateState();
        UpdateTargetDirection();
    }

    public virtual void FixedUpdate()
    {
        if (isRooted) return;
        StateMachine.Move();
    }

    // ==========================================
    // Enemy 고유 로직
    // ==========================================
    public void ChangeState(StateBase<EnemyStateMachine> nextState, bool force)
    {
        StateMachine.ChangeState(nextState, force);
    }

    internal void NotifyStateEntered(StateBase<EnemyStateMachine> state) => _enemyAnimator?.HandleStateEntered(this, state);
    internal void NotifyAttackWindupStarted() => _enemyAnimator?.HandleAttackWindupStarted();
    internal void NotifyAttackStrikeStarted() => _enemyAnimator?.HandleAttackStrikeStarted();
    internal void NotifyGrabStarted(float horizontalOffset) => _enemyAnimator?.HandleGrabStarted(this, horizontalOffset);

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    public bool isPlayerInVision() => Physics2D.CircleCast(transform.position, visionRange, Vector2.right, 0, LayerMask.GetMask("Player"));
    public bool isPlayerInAttackRange() => Physics2D.Raycast(transform.position, targetDir, startAttackRange, LayerMask.GetMask("Player"));
    public bool isMovingLeft() => rb.linearVelocityX * Vector2.left.magnitude > 0f;
    public bool IsCurrentState(StateBase<EnemyStateMachine> state) => StateMachine.IsCurrentState(state);

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
        AudioClip hitSound = isCurrentHitGrab ? grabHitSound : sweepHitSound;
        if (myAudioSource != null && hitSound != null) myAudioSource.PlayOneShot(hitSound);

        if (StateMachine.currentState.CanBeCancelled()) ReceiveKnockback(attackInfo, true);

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        currentPower = IsHitReactionState(CurrentState) ? attackInfo.power : 0;

        if (EmitManager.Instance.isEmitting) EventManager.TriggerEvent("OnEmitAborted");

        if (currentHealth <= 0)
        {
            if (SoundManager.instance != null && deathSound != null) SoundManager.instance.PlaySFX(deathSound);
            StageManager.Instance?.EnemyDied();
            ChangeState(DeadState, true);
            gameObject.SetActive(false);
        }
    }

    public override void ReceiveDamage(AttackInfo attackInfo)
    {
        int damage = DamageCalculation<Enemy>.Calculate(attackInfo, this);
        currentHealth -= damage;
    }

    public override void ReceiveKnockback(AttackInfo attackInfo, bool force)
    {
        currentKnockBackForce = attackInfo.knockback;
        isCurrentHitGrab = attackInfo.attackIsGrab;
        bool shouldForceChange = force || canChangeHitState;

        if (!isCurrentHitGrab) StateMachine.ChangeState(StateMachine.KnockbackState, force);
        else StateMachine.ChangeState(StateMachine.GrabbedState, force);
    }

    public override void ReceiveGrab()
    {
        if (CurrentState is SharedGrabbedState<EnemyStateMachine> grabState) grabState.Grab();
    }

    private bool IsHitReactionState(StateBase<EnemyStateMachine> state) => state == StateMachine.KnockbackState || state == StateMachine.GrabbedState;

    public void ResetPowerOutsideHitReaction()
    {
        if (!IsHitReactionState(CurrentState)) currentPower = 0;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class Boss1 : EnemyBase
{
    public BossStateMachine StateMachine { get; private set; }
    public StateBase<BossStateMachine> CurrentState => StateMachine?.CurrentState;

    private BossAnimator _bossAnimator;
    public LaserTargetting laserAttack;

    [Header("Boss 전용 설정")]
    [SerializeField] private List<Transform> hoverPoints;
    private float groggyGravityScale = 5f;
    public AudioClip hoverSound;
    public AudioClip groggySound;

    public override void Awake()
    {
        base.Awake();
        laserAttack = GetComponent<LaserTargetting>();
    }

    public override void Init()
    {
        base.Init();
        _bossAnimator = GetComponent<BossAnimator>();
        StateMachine = GetComponent<BossStateMachine>();
    }

    private void Start()
    {
        StageManager.Instance?.AddEnemy();
        StateMachine.ChangeState(StateMachine.MoveState);
    }

    private void Update()
    {
        if (EmitManager.Instance.isEmitting)
        {
            return;
        }

        StateMachine.UpdateState();
        UpdateTargetDirection();
    }

    private void FixedUpdate()
    {
        if (EmitManager.Instance.isEmitting) return;
        StateMachine.Move();
    }

    public void ChangeState(StateBase<BossStateMachine> nextState, bool force)
    {
        StateMachine.ChangeState(nextState, force);
    }

    internal void NotifyStateEntered(StateBase<BossStateMachine> state) => _bossAnimator?.HandleStateEntered(this, state);

    // ==========================================
    // Boss 전용 패턴 로직
    // ==========================================
    public bool TryGetRandomHoverPosition(out Vector2 hoverPosition)
    {
        if (hoverPoints == null || hoverPoints.Count == 0)
        {
            hoverPosition = rb.position;
            return false;
        }
        Transform hoverPoint = hoverPoints[Random.Range(0, hoverPoints.Count)];
        hoverPosition = hoverPoint.position;
        return true;
    }

    public void PrepareHovering()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    public void EnterGroggySound()  //최초 그로기 한번만 수행
    {
        myAudioSource.PlayOneShot(groggySound);
    }

    public void ExitGroggySound()  //그로기 -> move 상태로 전환되는 경우만 수행
    {
        myAudioSource.PlayOneShot(hoverSound);
    }

    public void StartTargetting()
    {
        laserAttack?.StartTargetingAttack();
        myAudioSource.PlayOneShot(attackReadySound);
    }

    public void StartAttack()
    {
        myAudioSource.PlayOneShot(attackSound);
    }

    public void EnterGroggy()
    {
        ResetPower();
        rb.gravityScale = groggyGravityScale;
        canChangeState = false;
    }

    public void ExitGroggy()
    {
        ResetPower();
        PrepareHovering();
        canChangeState = true;
    }

    // ==========================================
    // 추상 매서드 오버라이드
    // ==========================================
    public override void ReceiveAttack(AttackInfo attackInfo)
    {
        AudioClip hitSound = isCurrentHitGrab ? grabHitSound : sweepHitSound;
        if (myAudioSource != null && hitSound != null) myAudioSource.PlayOneShot(hitSound);

        if (StateMachine.CurrentState == StateMachine.RecoilState)
            StateMachine.ChangeState(StateMachine.GroggyState);

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        currentPower = IsHitReactionState(CurrentState) ? attackInfo.power : 0;

        if (StateMachine.CurrentState.CanBeCancelled()) ReceiveKnockback(attackInfo, true);

        if (EmitManager.Instance.isEmitting) EventManager.TriggerEvent("OnEmitAborted");

        if (currentHealth <= 0)
        {
            if (SoundManager.instance != null && deathSound != null) SoundManager.instance.PlaySFX(deathSound);
            StageManager.Instance?.EnemyDied();
            gameObject.SetActive(false);
        }
    }

    public override void ReceiveDamage(AttackInfo attackInfo)
    {
        int damage = DamageCalculation<Boss1>.Calculate(attackInfo, this);
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
        if (CurrentState is SharedGrabbedState<BossStateMachine> grabState) grabState.Grab();
    }
    public bool IsHitReactionState(StateBase<BossStateMachine> state) 
        => state == StateMachine.KnockbackState || state == StateMachine.GrabbedState || state == StateMachine.GroggyState;

    private void ResetPower()
    {
        currentPower = 0;
    }
}
using EMIT.Utilities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyState CurrentState => StateMachine?.CurrentState;

    public EnemyIdleState IdleState => StateMachine?.IdleState;
    public EnemyDeadState DeadState => StateMachine?.DeadState;

    public Transform target;

    private EnemyAnimator _enemyAnimator;
    public AudioSource myAudioSource;

    public ParticleSystem dragEffect;
    public EnemyOutlineController outlineController;


    [Header("적 데이터")]
    [SerializeField] private EnemyData data;

    public Rigidbody2D rb;
    public int maxHealth;
    public int baseAttack;
    public float baseSpeed;
    public float jumpHeight;
    public float visionRange;

    public float attackRange;
    public float startAttackRange;
    public float startUpTime;
    public float recoveryTime;

    public float knockbackRate;
    public float knockbackTime;
    public float grabForce;
    public float HitRecoveryTime;

    public Color defaultOutlineColor;
    public Color attackOutlineColor;
    public Color emitOutlineColor;

    public AudioClip walkSound;
    public AudioClip attackReadySound;
    public AudioClip attackSound;
    public AudioClip sweepHitSound;
    public AudioClip grabHitSound;
    public AudioClip deathSound;

    [Header("적 상태")]
    public bool canChangeState;
    public int currentHealth;
    public bool isRooted;
    public Vector2 currentKnockBackForce;
    public int currentPower { get; private set; }
    public bool isCurrentHitGrab;
    public bool canChangeHitState;

    public Vector2 front;
    public bool isFacingLeft = false;
    public int dmgType { get; set; }

    public Vector2 targetDir;
    public Vector2 FacingDir { get; private set; } = Vector2.right;

    public virtual void Awake() {

        Debug.Log("Enemy Awake");
        Init();
        StateMachine = new EnemyStateMachine(this);
    }

    public virtual void Start()
    {
        Debug.Log("Enemy is Ready");
        StageManager.Instance?.AddEnemy();

        ChangeState(IdleState, false);
    }

    public virtual void Update()
    {
        if (EmitManager.Instance.isEmitting) return;

        StateMachine.UpdateState();

        targetDir = Direction2D.FacingDir(target.position.x - rb.position.x);
        front = targetDir;
        UpdateFacingDirection();
    }

    public virtual void Init()
    {
        target = Player.InstanceTransform;

        _enemyAnimator = GetComponent<EnemyAnimator>();
        outlineController = GetComponent<EnemyOutlineController>();

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        if (!data)
        {
            Debug.LogError("Enemy is missing Enemy Data ScriptableObject!!");
        }
        else
        {   
            maxHealth = currentHealth = data.baseHealth;
            baseAttack = data.baseAttack;
            baseSpeed = data.baseSpeed;
            jumpHeight = data.jumpHeight;
            visionRange = data.visionRange;
            attackRange = data.attackRange;
            startAttackRange = data.startAttackRange;
            startUpTime = data.startUpTime;
            recoveryTime = data.recoveryTime;
            knockbackRate = data.knockbackRate;
            knockbackTime = data.knockbackTime;
            grabForce = data.grabForce;
            HitRecoveryTime = data.HitRecoveryTime;
            defaultOutlineColor = data.defaultOutlineColor;
            attackOutlineColor = data.attackOutlineColor;
            emitOutlineColor = data.emitOutlineColor;

            walkSound = data.walkSound;
            attackReadySound = data.attackReadySound;
            attackSound = data.attackSound;
            sweepHitSound = data.sweepHitSound;
            grabHitSound = data.grabHitSound;
            deathSound = data.deathSound;
        }

        canChangeState = true;
        isRooted = false;
        canChangeHitState = false;
    }

    internal void NotifyStateEntered(EnemyState state)
    {
        _enemyAnimator?.HandleStateEntered(this, state);
    }

    internal void NotifyAttackWindupStarted()
    {
        _enemyAnimator?.HandleAttackWindupStarted();
    }

    internal void NotifyAttackStrikeStarted()
    {
        _enemyAnimator?.HandleAttackStrikeStarted();
    }

    internal void NotifyGrabStarted(float horizontalOffset)
    {
        _enemyAnimator?.HandleGrabStarted(this, horizontalOffset);
    }

    internal void StopAnimationControl()
    {
        _enemyAnimator?.StopAnimControl();
    }

    internal void StartAnimationControl()
    {
        _enemyAnimator?.StartAnimControl();
    }

    public void ChangeState(EnemyState nextState, bool force)
    {
        StateMachine.ChangeState(nextState, force);
    }
    
    public void SetState(EnemyState nextState, int replayPower)
    {
        StateMachine.SetState(nextState);
        currentPower = IsHitReactionState(nextState) ? replayPower : 0;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    public virtual void FixedUpdate()
    {   
        if (isRooted) return;

        StateMachine.Move();
    }

    public void ReceiveAttack(AttackInfo attackInfo)
    {
        int incomingPower = GetEffectivePower(attackInfo.power);
        currentKnockBackForce = attackInfo.knockback;
        isCurrentHitGrab = attackInfo.attackIsGrab;

        AudioClip hitSound = isCurrentHitGrab ? grabHitSound : sweepHitSound;
        if (myAudioSource != null && hitSound != null)
        {
            myAudioSource.PlayOneShot(hitSound);
        }

        if (StateMachine.AttackState.CanBeCancelled())
        {
            StateMachine.AttackState.ExitState();
        }

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy is Dead!");
            if (SoundManager.instance != null && deathSound != null)
            {
                SoundManager.instance.PlaySFX(deathSound);
            }
            StageManager.Instance?.EnemyDied();

            ChangeState(DeadState, true);
            EventManager.TriggerEvent("OnEmitAborted");
            gameObject.SetActive(false);
            return;
        }

        if (EmitManager.Instance.isEmitting)
        {
            EventManager.TriggerEvent("OnEmitAborted");
        }

        if (canChangeHitState)
        {
            StateMachine.KnockbackState.stopKnockbackRoutine();
            StateMachine.GrabbedState.stopGrapRoutine();
            ReceiveKnockback(true);
        }
        else ReceiveKnockback(false);

        currentPower = IsHitReactionState(CurrentState)
            ? incomingPower
            : 0;
    }

    public void ReceiveDamage(AttackInfo atk)
    {
        int damage = DamageCalculation.Calculate(atk, this);
        
        currentHealth -= damage;
        Debug.Log($"Enemy가 데미지를 {damage} 입음");
    }

    private void HitEffectBasedOnDamageType(int dmgType)
    {
        switch(dmgType)
        {
            case 1:
                CameraShake.Instance.ShakeCamera(10f, 0.2f);
                return;
            case 2:
                CameraShake.Instance.ShakeCamera(20f, 0.3f);
                EffectManager.Instance.PlayHitEffect(transform.position, currentKnockBackForce * -1);
                return;
            case 3:
                CameraShake.Instance.ShakeCamera(40f, 0.1f);
                EffectManager.Instance.PlayHitEffect(transform.position, currentKnockBackForce * -1);
                LightManager.Instance?.BlinkLight(0.05f, 0.8f);
                TimeManager.Instance?.StopTimeFor(0.8f);

                return;
            default:
                Debug.Log("잘못된 데미지 타입");
                return;
        }
    }

    private int GetEffectivePower(int power)
    {
        return EmitManager.Instance.isEmitting ? -power : power;
    }

    private bool IsHitReactionState(EnemyState state)
    {
        return state == StateMachine.KnockbackState || state == StateMachine.GrabbedState;
    }

    public void ResetPowerOutsideHitReaction()
    {
        if (!IsHitReactionState(CurrentState))
        {
            currentPower = 0;
        }
    }

    public void ReceiveKnockback(bool force)
    {
        if (!isCurrentHitGrab)
            StateMachine.ChangeState(StateMachine.KnockbackState, force);
        else
            StateMachine.ChangeState(StateMachine.GrabbedState, force);
    }

    public void ReceiveGrab()
    {
        if (StateMachine.IsCurrentState(StateMachine.GrabbedState))
            StateMachine.GrabbedState.Grab();
    }

    public bool isPlayerInVision()
    {
        return Physics2D.CircleCast(transform.position, visionRange, Vector2.right, 0, LayerMask.GetMask("Player"));
    }

    public bool isPlayerInAttackRange()
    {
        return Physics2D.Raycast(transform.position, targetDir, startAttackRange, LayerMask.GetMask("Player"));

    }

    public void RootMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isRooted = true;
    }

    public void UnrootMovement()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        isRooted = false;
    }

    public bool IsCurrentState(EnemyState state)
    {
        return StateMachine.IsCurrentState(state);
    }

    private void UpdateFacingDirection()
    {
        if (canChangeState)
        {
            Flip();
        }
    }

    public void Flip()
    {
        SetFacingDirection(targetDir);
    }

    public void SetFacingDirection(Vector2 direction)
    {
        Vector2 nextFacingDir = Direction2D.FacingDir(direction);

        if (nextFacingDir == Vector2.zero || !Direction2D.IsOpposite(FacingDir, nextFacingDir)) return;

        FacingDir = nextFacingDir;
        isFacingLeft = FacingDir == Vector2.left;
        transform.rotation = Quaternion.Euler(0f, isFacingLeft ? 180f : 0f, 0f);
    }

    public void startDragEffect()
    {
        dragEffect.Play();
    }

    public void endDragEffect()
    {
        dragEffect.Stop();
    }

    public bool isMovingLeft()
    {
        return rb.linearVelocityX * Vector2.left.magnitude > 0f;
    }

    public void Animevent_playWalkSound()
    {
        if (myAudioSource == null || walkSound == null) return;

        myAudioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
        myAudioSource.PlayOneShot(walkSound);
        myAudioSource.pitch = 1.0f;
    }
}

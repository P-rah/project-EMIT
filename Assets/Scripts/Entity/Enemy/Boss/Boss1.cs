using EMIT.Utilities;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Boss1 : MonoBehaviour, IDamageable, IGrabbable, IKnockbackable
{
    public BossStateMachine StateMachine { get; private set; }
    public StateBase<BossStateMachine> CurrentState => StateMachine?.CurrentState;

    public Transform target { get; set; }
    public Rigidbody2D rb { get; set; }

    private BossAnimator _bossAnimator;
    public AudioSource myAudioSource;

    public ParticleSystem dragEffect;
    public OutlineController outlineController;

    public LaserTargetting laserAttack;

    [Header("Boss 설정")]
    [SerializeField] private List<Transform> hoverPoints;
    [SerializeField] private EnemyData data;

    private float groggyGravityScale = 5f;

    public int maxHealth;
    public int baseAttack;
    public float baseSpeed;
    public float jumpHeight;
    public float visionRange;

    public float attackRange;
    public float startAttackRange;
    public float startUpTime;
    public float recoveryTime;

    public float knockbackRate { get; set; }
    public float knockbackTime { get; set; }
    public float grabForce { get; set; }
    public float HitRecoveryTime { get; set; }

    public AudioClip walkSound;
    public AudioClip attackReadySound;
    public AudioClip attackSound;
    public AudioClip sweepHitSound;
    public AudioClip grabHitSound;
    public AudioClip deathSound;

    [Header("보스 상태")]
    public bool canChangeState { get; set; }
    public bool canChangeHitState { get; set; }
    public Vector2 currentKnockBackForce { get; set; }

    public int currentHealth;
    public bool isRooted;
    public int currentPower { get; private set; }
    public bool isCurrentHitGrab;

    public Vector2 front;
    public bool isFacingLeft = false;
    public int dmgType { get; set; }

    public Vector2 targetDir;
    public Vector2 FacingDir { get; private set; } = Vector2.right;

    private void Awake()
    {
        Debug.Log("Enemy Awake");
        Init();
        laserAttack = GetComponent<LaserTargetting>();
    }

    public virtual void Init()
    {
        target = Player.InstanceTransform;

        _bossAnimator = GetComponent<BossAnimator>();
        outlineController = GetComponent<OutlineController>();
        StateMachine = GetComponent<BossStateMachine>();

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

    private void Start()
    {
        StageManager.Instance?.AddEnemy();
        StateMachine.ChangeState(StateMachine.MoveState);
        outlineController.SetDefaultOutline();
    }

    private void Update()
    {
        if (EmitManager.Instance.isEmitting) return;

        StateMachine.UpdateState();

        targetDir = Direction2D.FacingDir(target.position.x - rb.position.x);
        front = targetDir;
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (EmitManager.Instance.isEmitting) return;

        StateMachine.Move();
    }

    public void ChangeState(BossState nextState, bool force)
    {
        StateMachine.ChangeState(nextState, force);
    }

    public void ReceiveAttack(AttackInfo attackInfo)
    {
        AudioClip hitSound = isCurrentHitGrab ? grabHitSound : sweepHitSound;
        if (myAudioSource != null && hitSound != null)
        {
            myAudioSource.PlayOneShot(hitSound);
        }

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        if (StateMachine.CurrentState == StateMachine.RecoilState)
            StateMachine.ChangeState(StateMachine.GroggyState);

        if (StateMachine.CurrentState.CanBeCancelled()) ReceiveKnockback(attackInfo, true);

        if (EmitManager.Instance.isEmitting)
        {
            EventManager.TriggerEvent("OnEmitAborted");
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy is Dead!");
            if (SoundManager.instance != null && deathSound != null)
            {
                SoundManager.instance.PlaySFX(deathSound);
            }
            StageManager.Instance?.EnemyDied();
            //사망 로직 시행

            gameObject.SetActive(false);
            return;
        }
    }

    public void ReceiveDamage(AttackInfo attackInfo)
    {
        int incomingPower = GetEffectivePower(attackInfo.power);
        currentPower = IsHitReactionState(CurrentState)
            ? incomingPower
            : 0;

        int damage = DamageCalculation<Boss1>.Calculate(attackInfo, this);

        currentHealth -= damage;
        Debug.Log($"Enemy가 데미지를 {damage} 입음");
    }

    private void HitEffectBasedOnDamageType(int dmgType)
    {
        switch (dmgType)
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

    public void ReceiveKnockback(AttackInfo attackInfo, bool force)
    {
        int incomingPower = GetEffectivePower(attackInfo.power);
        currentKnockBackForce = attackInfo.knockback;
        isCurrentHitGrab = attackInfo.attackIsGrab;

        currentPower = IsHitReactionState(CurrentState) ? incomingPower : 0;

        bool shouldForceChange = force || canChangeHitState;

        if (!isCurrentHitGrab)
            StateMachine.ChangeState(StateMachine.KnockbackState, force);
        else
            StateMachine.ChangeState(StateMachine.GrabbedState, force);
    }

    public void ReceiveGrab()
    {
        //패턴 매칭 사용
        if (CurrentState is SharedGrabbedState<BossStateMachine> grabState)
        {
            grabState.Grab();
        }
    }

    private int GetEffectivePower(int power)
    {
        return EmitManager.Instance.isEmitting ? -power : power;
    }

    private bool IsHitReactionState(StateBase<BossStateMachine> state)
    {
        return state == StateMachine.KnockbackState || state == StateMachine.GrabbedState;
    }

    public bool TryGetRandomHoverPosition(out Vector2 hoverPosition)
    {
        if (hoverPoints == null || hoverPoints.Count == 0)
        {
            Debug.LogError("Boss1에 Hover Point가 연결되지 않았습니다.", this);
            hoverPosition = rb.position;
            return false;
        }

        Transform hoverPoint = hoverPoints[Random.Range(0, hoverPoints.Count)];
        if (hoverPoint == null)
        {
            Debug.LogError("Boss1의 Hover Point 목록에 비어 있는 항목이 있습니다.", this);
            hoverPosition = rb.position;
            return false;
        }

        hoverPosition = hoverPoint.position;
        return true;
    }

    public void PrepareHovering()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    public void EnterGroggy()
    {
        Debug.Log("그로기");
        rb.gravityScale = groggyGravityScale;
    }

    public void ExitGroggy()
    {
        PrepareHovering();
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

    public void Animevent_playWalkSound()
    {
        if (myAudioSource == null || walkSound == null) return;

        myAudioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
        myAudioSource.PlayOneShot(walkSound);
        myAudioSource.pitch = 1.0f;
    }

    internal void NotifyStateEntered(StateBase<BossStateMachine> state)
    {
        _bossAnimator?.HandleStateEntered(this, state);
    }

    public void UnrootMovement()
    {
    }
}
using EMIT.Utilities;
using UnityEngine;

// ==========================================
// 적이라면 무조건 가져야 하는 조상 클래스
// ==========================================

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public abstract class EnemyBase : MonoBehaviour, IDamageable, IGrabbable, IKnockbackable, IEmitableTarget
{
    [Header("Base References")]
    public Transform target { get; set; }
    public Rigidbody2D rb { get; set; }
    public AudioSource myAudioSource;
    public ParticleSystem dragEffect;
    public DieFragment deathFragmentPrefab;
    public EmitManager emitManager { get; set; }
    private PowerStackUIController powerStackUIController { get; set; }
    public SpriteRenderer spriteRenderer { get; set; }

    [SerializeField] protected EnemyData data;

    [Header("Base Stats")]
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
    public float strongKnockbackRate { get; set; }
    public float knockbackTime { get; set; }
    public float grabForce { get; set; }
    public float HitRecoveryTime { get; set; }
    public float powerResetTime;

    [Header("Audio Clips")]
    public AudioClip walkSound;
    public AudioClip attackReadySound;
    public AudioClip attackSound;
    public AudioClip attackHitSound;
    //public AudioClip grabHitSound;
    public AudioClip deathSound;

    [Header("Base State Flags")]
    public bool canChangeState { get; set; }
    public bool canChangeHitState { get; set; }
    public Vector2 currentKnockBackForce { get; set; }

    public int currentHealth;
    public bool IsDead { get; private set; }
    public bool isRooted;
    public int currentPower { get; protected set; }
    private float powerTimer;
    public bool isCurrentHitGrab;
    public int dmgType { get; set; }
    public bool transitFlipPower { get; set; }
    public bool canHit;
    [SerializeField] private bool canAttack = true;

    public Vector2 front;
    public bool isFacingLeft = false;
    public Vector2 targetDir;
    public Vector2 FacingDir { get; private set; } = Vector2.right;
    public bool flipable = true;

    [Header("Invincibility")]
    public float invincibilityDuration;
    protected float invincibilityTimer = 0f;
    public bool IsInvincible => invincibilityTimer > 0f;

    [Header("Hit Effects")]
    private const float hitEffectResetTime = 0.2f;
    private static int _flashAmount = Shader.PropertyToID("_FlashAmount");
    private float hitEffectTimer = 0f;

    public IEnemyStateMachine StateMachine { get; set; }
    public IState CurrentState => StateMachine?.CurrentState;
    public IState IdleState => StateMachine?.IdleState;
    public IState DeadState => StateMachine?.DeadState;


    public virtual void Awake()
    {
        Init();
    }

    protected virtual void OnEnable()
    {
        EventManager.StartListening("OnEmitFinished", CheckDeath);
        if (myAudioSource != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterSfxSource(myAudioSource);
        }
    }

    protected virtual void OnDisable()
    {
        EventManager.StopListening("OnEmitFinished", CheckDeath);

        if (myAudioSource != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.UnregisterSfxSource(myAudioSource);
        }
    }

    public virtual void Init()
    {
        target = Player.InstanceTransform;
        rb = GetComponent<Rigidbody2D>();
        powerStackUIController = GetComponentInChildren<PowerStackUIController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null) rb.freezeRotation = true;

        if (data == null)
        {
            Debug.LogError($"{gameObject.name} is missing Enemy Data ScriptableObject!!");
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
            strongKnockbackRate = data.strongKnockbackRate;
            knockbackTime = data.knockbackTime;
            grabForce = data.grabForce;
            HitRecoveryTime = data.HitRecoveryTime;
            powerResetTime = data.powerResetTime;
            invincibilityDuration = data.invincibilityDuration;
        }

        canChangeState = true;
        isRooted = false;
        canChangeHitState = false;
    }

    // ==========================================
    // 방향 전환
    // ==========================================
    protected void UpdateTargetDirection()
    {
        targetDir = Direction2D.FacingDir(target.position.x - rb.position.x);
        front = targetDir;

        if (canChangeState && flipable)
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
        powerStackUIController.OwnerFacingDir(isFacingLeft);
    }

    // ==========================================
    // 이펙트 / 사운드
    // ==========================================
    protected void HitEffectBasedOnDamageType(int type)
    {
        hitEffectTimer = 0f;
        spriteRenderer.material.SetFloat(_flashAmount, 1f);

        switch (type)
        {
            case 1:
                CameraShake.Instance.ShakeCamera(10f, 0.2f);
                break;
            case 2:
                CameraShake.Instance.ShakeCamera(20f, 0.3f);
                EffectManager.Instance.PlayHitEffect(transform.position, currentKnockBackForce * -1);
                break;
            case 3:
                CameraShake.Instance.ShakeCamera(40f, 0.1f);
                EffectManager.Instance.PlayHitEffect(transform.position, currentKnockBackForce * -1);
                LightManager.Instance?.BlinkLight(0.05f, 0.8f);
                TimeManager.Instance?.StopTimeFor(0.8f);
                break;
            default:
                Debug.Log("잘못된 데미지 타입");
                break;
        }
    }

    public void startDragEffect() => dragEffect?.Play();
    public void endDragEffect() => dragEffect?.Stop();

    public void Animevent_playWalkSound()
    {
        if (myAudioSource == null || walkSound == null) return;
        Debug.Log($"현재 오디오 아웃풋 그룹: {myAudioSource.outputAudioMixerGroup?.name}", gameObject);
        myAudioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
        myAudioSource.PlayOneShot(walkSound);
        myAudioSource.pitch = 1.0f;
    }

    public void Animevent_playAttackReadySound()
    {
        if (myAudioSource == null || walkSound == null) return;
        myAudioSource.PlayOneShot(attackReadySound);
    }

    // ==========================================
    // 이동 제어
    // ==========================================

    public void RootMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isRooted = true;
    }

    public virtual void UnrootMovement()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        isRooted = false;
    }

    public bool isPlayerInVision() => Physics2D.CircleCast(transform.position, visionRange, Vector2.right, 0, LayerMask.GetMask("Player"));
    public bool isPlayerInAttackRange()
    {
        if (canAttack)
            return Physics2D.Raycast(transform.position, targetDir, startAttackRange, LayerMask.GetMask("Player"));
        else return false;
    }

    // ==========================================
    // 무적 타이머
    // ==========================================

    protected void UpdateInvincibilityTimer()
    {
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    public void ResetInvincibility()
    {
        invincibilityTimer = 0f;
    }

    // ==========================================
    // powerStack 제어
    // ==========================================

    public void SetPower(int power)
    {
        currentPower = power;
        powerStackUIController.SetStack(currentPower);
    }

    public void CalculatePower(AttackInfo attack)
    {
        if (currentPower < 5 && currentPower > -5 && currentPower * attack.power >= 0 && !attack.attackIsGrab)
        {
            SetPower(currentPower + attack.power);
        }
    }

    public void ResetPower()
    {
        SetPower(0);
    }

    public void FlipPower(float knockBackForce)
    {
        if(knockBackForce * currentPower < 0)
            SetPower(-currentPower);
    }

    public void CalculatePowerTimer()
    {
        powerTimer += Time.deltaTime;
        if(powerTimer > powerResetTime)
            ResetPower();
    }

    public void ResetPowerTImer(){ powerTimer = 0; }

    public void EnableAttackCancel()
    {
        if (CurrentState is EnemyAttackState attackState)
            attackState.SetCancellable(true);
    }

    public void CalculateHitEffectTimer()
    {
        hitEffectTimer += Time.fixedDeltaTime;
        if (hitEffectTimer > hitEffectResetTime)
        {
            spriteRenderer.material.SetFloat(_flashAmount, 0f);
        }
    }


    // 피해 적용 직후 또는 Emit 종료 시에만 사망을 확인한다.
    protected void CheckDeath()
    {
        if (IsDead || currentHealth > 0) return;
        if (EmitManager.Instance != null && EmitManager.Instance.isEmitting) return;

        IsDead = true;
        try
        {
            StopOverlapDetect();
            endDragEffect();
            OnDeath();
            if (SoundManager.Instance != null && deathSound != null)
                SoundManager.Instance.PlaySFX(deathSound);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, this);
        }

        try
        {
            if (StageManager.Instance != null) StageManager.Instance.EnemyDied();
        }
        finally
        {
            // 연출이나 스테이지 통지에 실패해도 본체는 남기지 않는다.
            Destroy(gameObject);
        }
    }

    // 연출만 확장한다. 처치 집계와 Destroy는 CheckDeath에서 한 번만 실행한다.
    protected virtual void OnDeath()
    {
        if (StateMachine != null && DeadState != null)
            StateMachine.ChangeState(DeadState, true);
    }

    // 추상 매서드
    public abstract void ReceiveAttack(AttackInfo attackInfo);
    public abstract void ReceiveDamage(AttackInfo attackInfo);
    public abstract void ReceiveKnockback(AttackInfo attackInfo, bool force);
    public abstract void ReceiveGrab();

    public virtual void StartOverlapDetect() { }
    public virtual void StopOverlapDetect() { }

    internal abstract void NotifyStateEntered(IState state);
    internal abstract void NotifyAttackWindupStarted();
    internal abstract void NotifyAttackStrikeStarted();
    internal abstract void NotifyGrabStarted(float horizontalOffset);
    public string CurrentStateToString()
    {
        return CurrentState.ToString();
    }
}

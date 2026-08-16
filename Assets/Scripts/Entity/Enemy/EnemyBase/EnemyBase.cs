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
    public EmitManager emitManager { get; set; }
    private PowerStackUIController powerStackUIController { get; set; }

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
    public float knockbackTime { get; set; }
    public float grabForce { get; set; }
    public float HitRecoveryTime { get; set; }
    public float powerResetTime;

    [Header("Audio Clips")]
    public AudioClip walkSound;
    public AudioClip attackReadySound;
    public AudioClip attackSound;
    public AudioClip sweepHitSound;
    public AudioClip grabHitSound;
    public AudioClip deathSound;

    [Header("Base State Flags")]
    public bool canChangeState { get; set; }
    public bool canChangeHitState { get; set; }
    public Vector2 currentKnockBackForce { get; set; }

    public int currentHealth;
    public bool isRooted;
    public int currentPower { get; protected set; }
    private float powerTimer;
    public bool isCurrentHitGrab;
    public int dmgType { get; set; }

    public Vector2 front;
    public bool isFacingLeft = false;
    public Vector2 targetDir;
    public Vector2 FacingDir { get; private set; } = Vector2.right;

    public virtual void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        target = Player.InstanceTransform;
        rb = GetComponent<Rigidbody2D>();
        powerStackUIController = GetComponentInChildren<PowerStackUIController>();

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
            knockbackTime = data.knockbackTime;
            grabForce = data.grabForce;
            HitRecoveryTime = data.HitRecoveryTime;
            powerResetTime = data.powerResetTime;
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
        powerStackUIController.setPowerStackFlip(isFacingLeft);
    }

    // ==========================================
    // 이펙트 / 사운드
    // ==========================================
    protected void HitEffectBasedOnDamageType(int type)
    {
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
        myAudioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
        myAudioSource.PlayOneShot(walkSound);
        myAudioSource.pitch = 1.0f;
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

    // ==========================================
    // powerStack 제어
    // ==========================================

    public void SetPower(int power)
    {
        currentPower = power;
        powerStackUIController.SetStack(currentPower);
    }

    public void CalculatePower(AttackInfo attckInfo)
    {
        if (currentPower < 5 && currentPower > -5 && currentPower * attckInfo.power >= 0)
        {
            SetPower(currentPower + attckInfo.power);
        }
    }

    public void ResetPower()
    {
        SetPower(0);
    }

    public void CalculatePowerTimer()
    {
        powerTimer += Time.deltaTime;
        if(powerTimer > powerResetTime)
            ResetPower();
    }

    public void ResetPowerTImer(){ powerTimer = 0; }

    // 추상 매서드
    public abstract void ReceiveAttack(AttackInfo attackInfo);
    public abstract void ReceiveDamage(AttackInfo attackInfo);
    public abstract void ReceiveKnockback(AttackInfo attackInfo, bool force);
    public abstract void ReceiveGrab();
}
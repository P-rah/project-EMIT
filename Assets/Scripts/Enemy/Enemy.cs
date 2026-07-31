using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyEmitController))]
public class Enemy : MonoBehaviour, IDamageable
{
    public EnemyState currentState { get; private set; }
    public string currentStateString;

    public Transform target; //플레이어의 transform

    private EnemyAnimator enemyAnim;
    public AudioSource myAudioSource;

    public ParticleSystem dragEffect;

    public EnemyIdleState IdleState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyDeadState DeadState { get; private set; }
    public EnemyKnockbackState KnockbackState { get; private set;}  //넉백(밀치기) 상태
    public EnemyGrabbedState GrabbedState { get; private set; }     //넉백이랑 당겨오는 상태랑 분리

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

    public AudioClip walkSound;
    public AudioClip attackReadySound;
    public AudioClip attackSound;
    public AudioClip sweepHitSound;
    public AudioClip grabHitSound;
    public AudioClip deathSound;

    [Header("적 상태")]
    public bool canChangeState; // 다음 프레임에서 State가 자동으로 변경 가능한지만 관여
    public int currentHealth;
    public bool isRooted;
    public Vector2 currentKnockBackForce;
    public int currentPower { get; private set; }
    public bool isUltCondition { get; set; }
    public int ultPower { get; set; }
    public bool isCurrentHitGrab;
    public bool canChangeHitState;

    public Vector2 front; // 적이 바라보는 방향 (넉백 관련 처리 시 필요)
    public bool isFacingLeft = false;

    private float ultTime = 1f;
    private Coroutine ultRoutine;
    public int dmgType { get; set; }

    public virtual void Awake() {

        Debug.Log("Enemy Awake");
        Init();
    }

    public virtual void Start()
    {
        Debug.Log("Enemy is Ready");
        StageManager.Instance.AddEnemy();   //적 카운드 증가

        ChangeState(IdleState, false);
    }

    public virtual void Update()
    {
        if (EmitManager.Instance.isEmitting) return;

        if (currentState != DeadState) // race condition 방지
        {        
            currentState.UpdateState();
            currentStateString = currentState.GetType().Name;
        }
        front = (rb.transform.position.x > target.transform.position.x) ? Vector2.left : Vector2.right;
        UpdateSpriteFlip();
    }

    public virtual void Init()
    {
        target = Player.InstanceTransform; //플레이어의 싱글톤 Transform 받아옴

        enemyAnim = GetComponent<EnemyAnimator>();

        IdleState = new EnemyIdleState(this, enemyAnim);
        AttackState = new EnemyAttackState(this, enemyAnim);
        ChaseState = new EnemyChaseState(this, enemyAnim);
        DeadState =  new EnemyDeadState(this, enemyAnim);
        KnockbackState = new EnemyKnockbackState(this, enemyAnim);
        GrabbedState = new EnemyGrabbedState(this, enemyAnim);

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

    public void ChangeState(EnemyState nextState, bool force)
    {
        if(!force)
        {
            if (currentState == nextState || !canChangeState) return;
        }

        currentState?.ExitState();
        currentState = nextState;
        ResetPowerOutsideHitReaction();
        currentState.EnterState();
    }
    
    /// <summary>
    /// ExitState와 EnterState를 발동시키지 않고 currentState만 바꾼다.
    /// </summary>
    public void SetState(EnemyState nextState, int replayPower)
    {
        currentState = nextState;
        currentPower = IsHitReactionState(currentState) ? replayPower : 0;
        currentStateString = currentState.GetType().Name;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        
        if (currentState == AttackState)
        {
            Vector3 dir = new Vector3(target.transform.position.x - transform.position.x, 0, 0).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + dir * attackRange);
        }
        
    }

    public void FixedUpdate()
    {   
        if (isRooted) return;

        currentState?.Move();
    }

    public void ReceiveAttack(AttackInfo attackInfo)
    {
        int incomingPower = GetEffectivePower(attackInfo.power);

        currentKnockBackForce = attackInfo.knockbackForce;
        isCurrentHitGrab = attackInfo.attackIsGrab;

        if(!isCurrentHitGrab) myAudioSource.PlayOneShot(sweepHitSound);
        else myAudioSource.PlayOneShot(grabHitSound);

        if (AttackState.CanBeCancelled())
        {
            AttackState.ExitState();
        }

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy is Dead!");
            SoundManager.instance.PlaySFX(deathSound);  //비활성화되면 소리 안나니까 얘만 매니저에서 재생
            StageManager.Instance.EnemyDied();

            ChangeState(DeadState, true);
            EventManager.TriggerEvent("OnEmitAborted");
            gameObject.SetActive(false);
            return;
        }

        if (EmitManager.Instance.isEmitting)
        {
            if (isUltCondition) EventManager.TriggerEvent("OnEmitUlted");
            else EventManager.TriggerEvent("OnEmitAborted");
        }

        if (canChangeHitState)
        {
            KnockbackState.stopKnockbackRoutine();
            GrabbedState.stopGrapRoutine();
            ReceiveKnockback(true);
        }
        else ReceiveKnockback(false);

        currentPower = IsHitReactionState(currentState)
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
                CameraShake.Instance.ShakeCamera(30f, 0.4f);
                EffectManager.Instance.PlayHitEffect(transform.position, currentKnockBackForce * -1);
                return;
            case 4:
                CameraShake.Instance.ShakeCamera(40f, 0.5f);
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
        return state == KnockbackState || state == GrabbedState;
    }

    private void ResetPowerOutsideHitReaction()
    {
        if (!IsHitReactionState(currentState))
        {
            currentPower = 0;
        }
    }

    public void ReceiveKnockback(bool force)
    {
        if (!isCurrentHitGrab)
            ChangeState(KnockbackState, force);
        else
            ChangeState(GrabbedState, force);
    }

    public void ReceiveGrab()
    {
        if(currentState == GrabbedState)
            GrabbedState.Grab();
    }

    public bool isPlayerInVision()
    {
        return Physics2D.CircleCast(transform.position, visionRange, Vector2.right, 0, LayerMask.GetMask("Player"));
    }

    public bool isPlayerInAttackRange()
    {
        return Physics2D.Raycast(transform.position, front, startAttackRange, LayerMask.GetMask("Player"));

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
        return currentState == state;
    }

    private void UpdateSpriteFlip()
    {
        if (canChangeState)
        {
            Flip();
        }
    }

    public void Flip()
    {
        if (isFacingLeft != (front == Vector2.left))
        {
            isFacingLeft = (front == Vector2.left);
            transform.rotation = Quaternion.Euler(0f, isFacingLeft ? 180f : 0f, 0f);
        }
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
    
    public void startUltTimer()
    {
        ultRoutine = StartCoroutine(UltTimerRoutine());
    }

    public void cancelUltTimer()
    {
        if (ultRoutine != null)
        {
            StopCoroutine(ultRoutine);
            isUltCondition = false;
            EventManager.TriggerEvent("OnEmitAborted");
            ultRoutine = null;
        }
    }

    public void Animevent_playWalkSound()
    {
        myAudioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
        myAudioSource.PlayOneShot(walkSound);
        myAudioSource.pitch = 1.0f;
    }

    private IEnumerator UltTimerRoutine()
    {
        yield return new WaitForSeconds(ultTime);
        isUltCondition = false;
        EventManager.TriggerEvent("OnEmitAborted");

        ultRoutine = null;
    }
}

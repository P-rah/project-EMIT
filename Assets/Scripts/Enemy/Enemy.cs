using System.Collections;
using EMIT.Utilities;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyState CurrentState => StateMachine?.CurrentState;

    // main 병합용 호환 프로퍼티. main이 사용하는 이름을 StateMachine 상태에 연결한다.
    public EnemyIdleState IdleState => StateMachine?.IdleState;
    public EnemyDeadState DeadState => StateMachine?.DeadState;

    /*
     * POST-MERGE:
     * 1. IdleState/DeadState 호환 프로퍼티를 사용하는 main 코드가 모두 StateMachine 직접 접근으로
     *    바뀌면 두 프로퍼티를 제거한다.
     * 2. front/isFacingLeft는 main 및 직렬화 호환용이다. 프리팹 마이그레이션 후 모든 참조를
     *    targetDir/FacingDir로 바꾸고 두 필드를 제거한다.
     * 3. StageManager의 적 등록/사망 집계는 풀링 환경에서 중복 호출되지 않는지 확인한 뒤,
     *    필요하면 Start 기반 등록을 OnEnable/OnDisable 기반으로 변경한다.
     * 4. LightManager/TimeManager 연출 수치는 플레이 테스트 후 EnemyData로 이동할 수 있다.
     * 5. 궁극기를 복구하려면 주석 처리된 필드/타이머와 DamageCalculation의 같은 방향 분기를
     *    한 번에 복구하고 Direction2D 기반 판정을 적용한다.
     * 6. main 호환을 위해 isMovingLeft는 기존 식을 유지한다. 병합 후 실제 왼쪽 이동 판정이
     *    필요하면 반환식을 rb.linearVelocityX < 0f로 변경한다.
     * 7. main과 동일하게 유지한 "//else " 줄의 후행 공백은 병합 완료 후 제거한다.
     */

    public Transform target; //플레이어의 transform

    public EnemyAnimator enemyAnim;
    public AudioSource myAudioSource;

    public ParticleSystem dragEffect;

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
    public bool isCurrentHitGrab;
    public bool canChangeHitState;

    //public bool isUltCondition { get; set; }
    //public int ultPower { get; set; }

    public Vector2 front; // 적이 바라보는 방향 (넉백 관련 처리 시 필요)
    public bool isFacingLeft = false;

    //private float ultTime = 1f;
    //private Coroutine ultRoutine;
    public int dmgType { get; set; }

    public Vector2 targetDir; // 플레이어가 있는 수평 방향
    public Vector2 FacingDir { get; private set; } = Vector2.right;

    public virtual void Awake() {

        Debug.Log("Enemy Awake");
        Init();
        StateMachine = new EnemyStateMachine(this);
    }

    public virtual void Start()
    {
        Debug.Log("Enemy is Ready");
        // StageManager.Instance.AddEnemy();   //적 카운드 증가

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
        target = Player.InstanceTransform; //플레이어의 싱글톤 Transform 받아옴

        enemyAnim = GetComponent<EnemyAnimator>();
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

            // walkSound = data.walkSound;
            // attackReadySound = data.attackReadySound;
            // attackSound = data.attackSound;
            // sweepHitSound = data.sweepHitSound;
            // grabHitSound = data.grabHitSound;
            // deathSound = data.deathSound;
}

        canChangeState = true;
        isRooted = false;
        canChangeHitState = false;
    }

    public void ChangeState(EnemyState nextState, bool force)
    {
        StateMachine.ChangeState(nextState, force);
    }
    
    /// <summary>
    /// ExitState와 EnterState를 발동시키지 않고 currentState만 바꾼다.
    /// </summary>
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

        // if(!isCurrentHitGrab) myAudioSource.PlayOneShot(sweepHitSound);
        // else myAudioSource.PlayOneShot(grabHitSound);

        if (StateMachine.AttackState.CanBeCancelled())
        {
            StateMachine.AttackState.ExitState();
        }

        ReceiveDamage(attackInfo);
        HitEffectBasedOnDamageType(dmgType);

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy is Dead!");
            // SoundManager.instance.PlaySFX(deathSound);  //비활성화되면 소리 안나니까 얘만 매니저에서 재생
            // StageManager.Instance.EnemyDied();

            ChangeState(DeadState, true);
            EventManager.TriggerEvent("OnEmitAborted");
            gameObject.SetActive(false);
            return;
        }

        if (EmitManager.Instance.isEmitting)
        {
            //if (isUltCondition) EventManager.TriggerEvent("OnEmitUlted");
            //else 
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

                // LightManager.Instance.BlinkLight(0.05f, 0.8f);
                // TimeManager.Instance.StopTimeFor(0.8f);

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
        // myAudioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
        // myAudioSource.PlayOneShot(walkSound);
        // myAudioSource.pitch = 1.0f;
    }

    //public void startUltTimer()
    //{
    //    ultRoutine = StartCoroutine(UltTimerRoutine());
    //}

    //public void cancelUltTimer()
    //{
    //    if (ultRoutine != null)
    //    {
    //        StopCoroutine(ultRoutine);
    //        isUltCondition = false;
    //        EventManager.TriggerEvent("OnEmitAborted");
    //        ultRoutine = null;
    //    }
    //}

    //private IEnumerator UltTimerRoutine()
    //{
    //    yield return new WaitForSeconds(ultTime);
    //    isUltCondition = false;
    //    EventManager.TriggerEvent("OnEmitAborted");

    //    ultRoutine = null;
    //}
}

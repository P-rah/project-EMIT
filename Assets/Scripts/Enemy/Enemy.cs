using System.Collections;
using EMIT.Utilities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    /* INTEGRATION:
     * 이 브랜치의 핵심 변경은 Enemy가 상태를 직접 생성/보관/전환하던 책임을 EnemyStateMachine으로
     * 분리한 것이다. CurrentState는 읽기 전용 진입점이며 모든 전환은 StateMachine을 거쳐 ExitState,
     * currentPower 초기화, EnterState 순서를 한 곳에서 보장한다. 통합 충돌 해결 시 integration 브랜치의
     * currentState 필드와 Enemy 내부 상태 인스턴스들을 되살리지 말고 아래 구조를 기준으로 삼는다.
     */
    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyState CurrentState => StateMachine?.CurrentState;

    /* INTEGRATION:
     * IdleState/DeadState는 기존 호출부를 단계적으로 StateMachine API로 옮기기 위한 읽기 전용 호환
     * 프로퍼티다. integration/enemy-state-merge의 PlayerCombat 등 모든 참조를 StateMachine 경로로
     * 바꾼 뒤 제거할 수 있지만, 이번 통합 중에는 Start와 사망 전환이 사용하므로 유지한다.
     */
    public EnemyIdleState IdleState => StateMachine?.IdleState;
    public EnemyDeadState DeadState => StateMachine?.DeadState;

    public Transform target; //플레이어의 transform

    private EnemyAnimator _enemyAnimator;
    public AudioSource myAudioSource;

    public ParticleSystem dragEffect;
    public EnemyOutlineController outlineController;

    /* INTEGRATION:
     * integration/enemy-state-merge에는 EnemyOutlineController와 기본/공격/역행 색상 필드가 추가된다.
     * 해당 필드와 Init의 EnemyData 색상 복사는 이 영역에 가져온다. EnemyAnimator는 선택적 프레젠테이션
     * 컴포넌트로만 보관하며 외곽선과 애니메이터가 없는 테스트 프리팹도 null-safe하게 동작해야 한다.
     */

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

    /* INTEGRATION:
     * front/isFacingLeft는 기존 프리팹과 integration 브랜치 코드의 직렬화/호출 호환을 위해 남겨 둔 값이다.
     * 실제 방향의 기준은 targetDir/FacingDir이며 Update와 SetFacingDirection에서 호환 값을 동기화한다.
     * 통합 후 프리팹과 모든 호출부를 마이그레이션하기 전에는 이 두 필드를 삭제하거나 별도로 갱신하지 않는다.
     */

    //private float ultTime = 1f;
    //private Coroutine ultRoutine;
    public int dmgType { get; set; }

    public Vector2 targetDir; // 플레이어가 있는 수평 방향
    public Vector2 FacingDir { get; private set; } = Vector2.right;

    public virtual void Awake() {

        Debug.Log("Enemy Awake");
        Init();
        /* INTEGRATION:
         * integration 브랜치의 Init 내부 개별 new Enemy...State 코드는 모두 제거하고 이 한 줄만 상태 생성
         * 지점으로 유지한다. State는 선택적 EnemyAnimator를 캡처하지 않고 Enemy mediator만 사용한다.
         */
        StateMachine = new EnemyStateMachine(this);
    }

    public virtual void Start()
    {
        Debug.Log("Enemy is Ready");
        /* INTEGRATION:
         * integration 브랜치의 StageManager 적 등록은 유지해야 하지만 현재 브랜치에서는 보스/테스트 씬에
         * StageManager가 없을 수 있어 비활성화되어 있다. 병합 시 StageManager 수명주기를 먼저 보장한 뒤
         * AddEnemy를 복구하고, 오브젝트 풀링을 쓴다면 Start가 재활성화마다 호출되지 않는 점을 고려해
         * OnEnable 기반 등록 또는 중복 방지 정책을 선택한다.
         */
        StageManager.Instance?.AddEnemy();   //적 카운드 증가

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
            /* INTEGRATION:
             * 사운드 필드는 이 브랜치의 보스/리팩터 테스트 프리팹에서 AudioSource 또는 클립이 비어 있어
             * null 재생을 피하려고 임시 비활성화했다. integration 브랜치에는 실제 EnemyData 클립과 재생
             * 경로가 있으므로 프리팹 참조를 확인한 뒤 여섯 대입과 피격/보행 재생을 한 묶음으로 복구한다.
             */
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
        /* INTEGRATION:
         * 외부 호출 호환용 파사드다. 상태 변경 규칙을 중복 구현하지 않고 StateMachine에 위임해야 강제
         * 전환 여부와 currentPower 초기화가 모든 경로에서 동일하다.
         */
        StateMachine.ChangeState(nextState, force);
    }
    
    /// <summary>
    /// ExitState와 EnterState를 발동시키지 않고 currentState만 바꾼다.
    /// </summary>
    public void SetState(EnemyState nextState, int replayPower)
    {
        /* INTEGRATION:
         * 역행 재생은 과거 상태를 복원할 때 EnterState/ExitState의 코루틴·이펙트를 다시 실행하면 안 되므로
         * 일반 ChangeState가 아닌 부작용 없는 SetState를 사용한다. replayPower는 넉백/그랩 상태일 때만
         * 보존해 이후 연계 피해 방향을 복원한다. EnemyEmitController와 반드시 같은 규약으로 유지한다.
         */
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

        /* INTEGRATION:
         * AttackInfo의 벡터 필드는 이 브랜치에서 knockback으로 이름을 통일했고 Boss1도 같은 계약을 쓴다.
         * integration 브랜치의 knockbackForce 참조를 남기지 말고 공격 히트박스와 피격 대상 모두 knockback
         * 하나로 맞춘다. power는 역행 중 부호를 뒤집은 뒤 현재 피격 상태에만 저장한다.
         */
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
            /* INTEGRATION:
             * integration 브랜치의 deathSound와 StageManager.EnemyDied 호출은 이 위치에 복구한다. 비활성화
             * 전에 SoundManager를 통해 재생해야 AudioSource 종료로 소리가 잘리지 않는다. 풀링/중복 피격으로
             * EnemyDied가 두 번 집계되지 않도록 사망 상태 진입 또는 별도 플래그로 한 번만 호출되게 한다.
             */
            // if (AudioManager.Instance != null && deathSound != null)
            if (SoundManager.instance != null && deathSound != null)
            {
                // AudioManager.Instance.PlaySFX(deathSound);  //비활성화되면 소리 안나니까 얘만 매니저에서 재생
                SoundManager.instance.PlaySFX(deathSound);  //비활성화되면 소리 안나니까 얘만 매니저에서 재생
            }
            StageManager.Instance?.EnemyDied();

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

                /* INTEGRATION:
                 * integration 브랜치의 LightManager/TimeManager 연출은 역행 반대 방향 연계인 dmgType 3에만
                 * 추가한다. 해당 매니저가 없는 테스트 씬에서도 동작해야 한다면 존재 여부를 보장하거나
                 * 안전 호출로 통합한다. 0.05/0.8초 수치는 플레이 테스트 후 EnemyData로 이동할 수 있다.
                 */
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
        /* INTEGRATION:
         * 방향을 transform 회전, isFacingLeft, 역행 프레임이 각자 계산하지 않도록 Direction2D로 정규화한
         * 단일 갱신 지점이다. 0 방향에서는 기존 방향을 유지하고 실제 좌우가 바뀔 때만 회전한다.
         * integration 브랜치의 UpdateSpriteFlip 구현 대신 이 메서드를 유지하며 외곽선 로직과 독립시킨다.
         */
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
        /* INTEGRATION:
         * 이 반환식은 공통 조상의 기존 동작을 그대로 보존했지만 Vector2.left.magnitude는 1이므로 실제로는
         * 'x 속도가 양수인가'를 검사한다. 통합 과정에서 호출 의미가 진짜 왼쪽 이동 판정임을 확인했다면
         * rb.linearVelocityX < 0f로 별도 수정하고, 이번 주석 전용 병합에서는 실행 로직을 바꾸지 않는다.
         */
        return rb.linearVelocityX * Vector2.left.magnitude > 0f;
    }

    public void Animevent_playWalkSound()
    {
        if (myAudioSource == null || walkSound == null) return;

        myAudioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
        myAudioSource.PlayOneShot(walkSound);
        myAudioSource.pitch = 1.0f;
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

using System.Collections;
using UnityEngine;
/*
 * 플레이어 공격, 역행 관리용 클래스
 */
public class PlayerCombat : MonoBehaviour
{
    private Player player;
    private PlayerAnimation animation;
    private PlayerInputHandler input;
    private PlayerState state;
    private GrapEffect grapEffect;
    private EmitManager emitManager;
    private TempPlayerMovement playerMovement;
    private SoundManager soundManager;

    [Header("Hitbox Colliders")]
    public PlayerAttackHitbox[] sweepAttackHitbox;
    public PlayerGrapHitbox grabAttackHitbox;

    [Header("Attack Settings")]
    [SerializeField] private int maxSweepCount = 3;
    public int sweepCount;
    [SerializeField] private float continuousSweepTime = 0.3f;
    private Coroutine sweepTimeRoutine;
    private bool checkContinuousSweep;      //연속 공격 체크 시작
    private bool triggerContinuousSweep;    //연속 공격의 트리거
    private bool checkCancelSweep;          //대쉬 체크 시작
    private bool TriggerCancelSweep;        //대쉬로 인한 캔슬 트리거
    private bool StartCancelSweep;          //캔슬 시작
    private bool sweepBufferStart = false;
    private bool sweepBuffered = false;

    [Header("Emit Settings")]
    const float emitRecoveryAmountWhenAttackHit = 0.2f;
    private bool hasTriggeredEmit;

    private bool canAttack = true;
    private bool hasTriggeredHitEffect = false;

    public bool IsAttackingSweep { get; private set; }
    public bool IsAttackingGrab { get; private set; }

    private IGrabbable currentGrabTarget;

    private Transform currentTargetTransform;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        state = Player.State;
        grapEffect = Player.GrapEffect;
        animation = Player.Animation;
        sweepCount = 0;
        playerMovement = Player.TempMovement;
    }

    private void Start()
    {
        emitManager = EmitManager.Instance;
        soundManager = SoundManager.Instance;
        player = Player.Instance;
    }

    private void OnEnable()
    {
        EventManager.StartListening("OnEmitStart", OnEmitStart);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnEmitStart", OnEmitStart);
    }

    private void Update()
    {
        if (state.IsDead) return;
        CheckInputs();
        CheckEmitable();
    }

    private void CheckInputs()
    {
        if (!input.UsingInputBuffer && checkCancelSweep)
        {
            if (input.DashInput)
            {
                TriggerCancelSweep = true;
            }
            if (StartCancelSweep && TriggerCancelSweep)
            {
                StartCancelSweep = false;
                TriggerCancelSweep = false;
                triggerContinuousSweep = false;
                IsAttackingSweep = false;
                animation.CancelAttackSweep();
            }
        }
        if (input.ConsumeAttackSweep())
        {
            ExecuteAttackSweep();
        }
        if (input.ConsumeAttackGrab())
        {
            ExecuteAttackGrab();
        }
    }

    public void ExecuteAttackSweep()
    {
        if (canAttack)
        {
            if (sweepTimeRoutine != null) StopCoroutine(sweepTimeRoutine);
            triggerContinuousSweep = false;
            if (state.CurrentState == State.AttackingGrab)
            {
                AnimEvent_AttackGrapEnd();
            }
            canAttack = false;
            IsAttackingSweep = true;
            hasTriggeredHitEffect = false;

            if (sweepCount >= maxSweepCount) sweepCount = 1;
            else sweepCount++;
            animation.SetAttackSweep(sweepCount);
        }
        else
        {
            if(checkContinuousSweep&&!TriggerCancelSweep)
            {
                triggerContinuousSweep = true;
            }
            else if(sweepBufferStart)
            {
                sweepBuffered = true;
            }
        }
    }

    // 기존 플래그 기반 캔슬 경로는 유지하고, 버퍼 경로는 대시 시작 직전에만 호출한다.
    public bool TryCancelSweepForBufferedDash()
    {
        if (!IsAttackingSweep || !checkCancelSweep || !StartCancelSweep) return false;

        StartCancelSweep = false;
        TriggerCancelSweep = false;
        triggerContinuousSweep = false;
        checkContinuousSweep = false;
        checkCancelSweep = false;
        IsAttackingSweep = false;
        playerMovement.flippable = true;
        animation.CancelAttackSweep();
        return true;
    }

    public void PlayComboEffect(int comboIndex)
    {
        if (comboIndex < 0 || comboIndex >= sweepAttackHitbox.Length) return;

        sweepAttackHitbox[comboIndex].gameObject.SetActive(true);
        sweepAttackHitbox[comboIndex].OnTargetSweepHit += handleSweepHit;
    }

    public void EndComboEffect(int comboIndex)
    {
        if (comboIndex < 0 || comboIndex >= sweepAttackHitbox.Length) return;

        sweepAttackHitbox[comboIndex].gameObject.SetActive(false);
        sweepAttackHitbox[comboIndex].OnTargetSweepHit -= handleSweepHit;
    }

    //공격 판정 시작
    public void AnimEvent_AttackSweepStart()
    {
        playerMovement.flippable = false;
        PlayComboEffect(sweepCount - 1);
        soundManager.PlaySFX(player.sweepSound);
        playerMovement.TriggerMoveWhileAttack();
        checkCancelSweep = true;
    }

    //공격 버퍼링
    public void AnimEvent_StartCheckingNextCondition()
    {
        checkContinuousSweep = true;
    }

    //공격 판정 끝
    public void AnimEvent_AttackSweepEnd()
    {
        playerMovement.flippable = true;
        EndComboEffect(sweepCount - 1);
        StartCancelSweep = true;
    }

    //애니메이션이 어떤 경우든 종료되면 실행
    public void AttackSweepReset()
    {
        if (input.UsingInputBuffer)
        {
            StartCancelSweep = false;
            TriggerCancelSweep = false;
        }
        checkContinuousSweep = false;
        checkCancelSweep = false;
        canAttack = true;
        if (triggerContinuousSweep)
        {
            IsAttackingSweep = true;
            ExecuteAttackSweep();
        }
        else
        {
            IsAttackingSweep = false;
            sweepTimeRoutine = StartCoroutine(continuousSweepTimer());
        }
    }

    private IEnumerator continuousSweepTimer()
    {
        yield return new WaitForSeconds(continuousSweepTime);
        sweepCount = 0;
        sweepTimeRoutine = null;
    }

    private void handleSweepHit(Transform target, Vector2 hitPoint)
    {
        if (!hasTriggeredHitEffect && !emitManager.isEmitting)
        {
            //공격이 적중했을 떄
            hasTriggeredHitEffect = true;
            emitManager.AddEmitTime(emitRecoveryAmountWhenAttackHit);
            soundManager.PlaySFX(player.sweepHitSound);
            Debug.Log("공격명중");
        }
    }

    public void ExecuteAttackGrab()
    {
        if (canAttack)
        {
            canAttack = false;
            IsAttackingGrab = true;
            hasTriggeredHitEffect = false;
            Player.TempMovement.flippable = false;

            grabAttackHitbox.OnTargetGrapHit += handleGrapHit;
        }
    }

    private void handleGrapHit(Transform target, Vector2 hitPoint)
    {
        currentGrabTarget = target.GetComponent<IGrabbable>();

        if(currentGrabTarget != null)
        {
            currentTargetTransform = target;
            if (!hasTriggeredHitEffect && !emitManager.isEmitting)
            {
                //공격이 적중했을 때
                hasTriggeredHitEffect = true;
                emitManager.AddEmitTime(emitRecoveryAmountWhenAttackHit);
                soundManager.PlaySFX(player.grabHitSound);
                Debug.Log("그랩명중");
            }
        }
    }

    public void AnimEvent_AttackGrapEnd()
    {
        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        currentTargetTransform = null;
        currentGrabTarget = null;
        grapEffect.StopGrabVisual();

        IsAttackingGrab = false;
        //Player.Movement.flippable = true;
        Player.TempMovement.flippable = true;

        canAttack = true;
        if (sweepBuffered)
        {
            sweepBuffered = false;
            ExecuteAttackSweep();
        }
        sweepBufferStart = false;
    }

    public void AnimEvent_GrabHitboxActive()
    {
        grabAttackHitbox.CatchClosestEnemy();
        startGrabVisual();
        soundManager.PlaySFX(player.grabThrowSound1);
        soundManager.PlaySFX(player.grabThrowSound2);
    }

    private void startGrabVisual()
    {
        //bool isTargetAlive = currentGrabTarget == null
        //    || currentGrabTarget.CurrentState != currentGrabTarget.StateMachine.DeadState;

        if (currentTargetTransform != null)
            grapEffect.StartGrabVisual(currentTargetTransform);
        else grapEffect.StartGrabFailVisual();
    }

    public void AnimEvent_StartSweepInputBuffer()
    {
        sweepBufferStart = true;
    }

    public void AnimEvent_startEnemyGrab()
    {
        if (currentTargetTransform != null && currentGrabTarget != null)
            currentGrabTarget.ReceiveGrab();
        else if (currentTargetTransform == null)
        {
            currentGrabTarget = null;
            grapEffect.StartGrabFailVisualRetract();
        }
        soundManager.PlaySFX(player.grabRetreiveSound);
        canAttack = true;
    }

    private void OnEmitStart()
    {
        hasTriggeredEmit = false;
    }

    private void CheckEmitable()
    {
        //역행 가능상태가 되었을 시 한번만 동작
        if(emitManager.isEmittable && !hasTriggeredEmit)
        {
            soundManager.PlaySFX(player.canEmitSound);
            hasTriggeredEmit = true;
        }
    }
}

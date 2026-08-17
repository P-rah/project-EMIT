using System.Collections;
using UnityEngine;
/*
 * 플레이어 공격, 역행 관리용 클래스
 */
public class NewPlayerCombat : MonoBehaviour
{
    private enum SweepType
    {
        Short,
        Long
    }

    private PlayerInputHandler input;
    private PlayerState state;
    private PlayerAnimation playerAnimation;
    private GrapEffect grapEffect;

    [Header("Hitbox Colliders")]
    public PlayerAttackHitbox shortSweep;
    public PlayerAttackHitbox longSweep;
    public PlayerGrapHitbox grabAttackHitbox;

    [Header("Attack Settings")]
    [Min(0f)]
    public float attackCoolTime = 0.3f;

    [Header("Sweep Timing")]
    [Min(0f)] public float sweepHitboxStartTime = 0.083333336f;
    [Min(0f)] public float sweepHitboxEndTime = 0.25f;
    [Min(0f)] public float sweepAnimationDuration = 0.33333334f;
    [Min(0f)] public float longSweepInputWindow = 0.5f;

    [Header("Grab Timing")]
    [Min(0f)] public float grabHitTime = 0.083333336f;
    [Min(0f)] public float grabResolveTime = 0.25f;
    [Min(0f)] public float grabAnimationDuration = 0.5f;

    [Header("Emit Settings")]
    public int emitRecoveryCount = 5;
    public int currentEmitRecoveryCount = 0;
    private bool hasTriggeredEmit;

    private bool canAttackSweep = true;
    private bool canAttackGrap = true;
    private bool hasTriggeredHitEffect = false;
    private bool longSweepQueued;

    private float longSweepInputDeadline = float.NegativeInfinity;

    private SweepType? activeSweepType;
    private PlayerAttackHitbox activeSweepHitbox;
    private Coroutine sweepAttackRoutine;
    private Coroutine sweepCooldownRoutine;
    private Coroutine grabAttackRoutine;

    private IGrabbable currentGrabTarget;

    private Transform currentTargetTransform;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        state = Player.State;
        playerAnimation = GetComponent<PlayerAnimation>();
        grapEffect = Player.GrapEffect;

        currentEmitRecoveryCount = emitRecoveryCount;
    }

    private void OnEnable()
    {
        EventManager.StartListening("OnEmitPressed", OnEmitPressed);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnEmitPressed", OnEmitPressed);
        CancelAllAttacks();
    }

    private void Update()
    {
        if (Player.State.isDeath) return;
        CheckInputs();
        CheckEmitable();
    }

    private void CheckInputs()
    {
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
        if (activeSweepType == SweepType.Short)
        {
            if (Time.time <= longSweepInputDeadline)
                longSweepQueued = true;

            return;
        }

        if (activeSweepType == SweepType.Long)
            return;

        bool shouldUseLongSweep = Time.time <= longSweepInputDeadline;

        if (grabAttackRoutine != null)
        {
            if (!canAttackSweep)
                return;

            CancelGrabAttack();
        }

        if (!shouldUseLongSweep && !canAttackSweep)
            return;

        StartSweepAttack(shouldUseLongSweep ? SweepType.Long : SweepType.Short);
    }

    private void StartSweepAttack(SweepType sweepType)
    {
        if (sweepCooldownRoutine != null)
        {
            StopCoroutine(sweepCooldownRoutine);
            sweepCooldownRoutine = null;
        }

        activeSweepType = sweepType;
        longSweepQueued = false;

        if (sweepType == SweepType.Short)
            longSweepInputDeadline = Time.time + longSweepInputWindow;
        else
            longSweepInputDeadline = float.NegativeInfinity;

        canAttackSweep = false;
        canAttackGrap = false;
        state.IsAttackingSweep = true;
        hasTriggeredHitEffect = false;
        Player.Movement.flippable = false;

        if (sweepType == SweepType.Short)
            playerAnimation.PlayShortSweep();
        else
            playerAnimation.PlayLongSweep();

        sweepAttackRoutine = StartCoroutine(SweepAttackRoutine(sweepType));
    }

    private IEnumerator SweepAttackRoutine(SweepType sweepType)
    {
        yield return new WaitForSeconds(sweepHitboxStartTime);

        OpenSweepHitbox(sweepType == SweepType.Short ? shortSweep : longSweep);

        float hitboxDuration = Mathf.Max(0f, sweepHitboxEndTime - sweepHitboxStartTime);
        yield return new WaitForSeconds(hitboxDuration);

        CloseSweepHitbox();

        float recoveryDuration = Mathf.Max(0f, sweepAnimationDuration - sweepHitboxEndTime);
        yield return new WaitForSeconds(recoveryDuration);

        sweepAttackRoutine = null;

        if (sweepType == SweepType.Short && longSweepQueued)
        {
            StartSweepAttack(SweepType.Long);
            yield break;
        }

        FinishSweepAttack();
    }

    private void OpenSweepHitbox(PlayerAttackHitbox hitbox)
    {
        activeSweepHitbox = hitbox;

        if (activeSweepHitbox == null)
        {
            Debug.LogError($"{name}: {activeSweepType} Sweep Hitbox가 할당되지 않았습니다.", this);
            return;
        }

        activeSweepHitbox.OnTargetSweepHit -= handleSweepHit;
        activeSweepHitbox.OnTargetSweepHit += handleSweepHit;
        activeSweepHitbox.gameObject.SetActive(true);
        SoundManager.instance.PlaySFX(Player.Instance.sweepSound);
    }

    private void CloseSweepHitbox()
    {
        if (activeSweepHitbox == null)
            return;

        activeSweepHitbox.gameObject.SetActive(false);
        activeSweepHitbox.OnTargetSweepHit -= handleSweepHit;
        activeSweepHitbox = null;
    }

    private void FinishSweepAttack()
    {
        CloseSweepHitbox();

        activeSweepType = null;
        state.IsAttackingSweep = false;
        Player.Movement.flippable = true;
        canAttackGrap = true;

        sweepCooldownRoutine = StartCoroutine(SweepCooldownRoutine());
    }

    private IEnumerator SweepCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCoolTime);

        canAttackSweep = true;
        sweepCooldownRoutine = null;
    }

    private void handleSweepHit(Transform target, Vector2 hitPoint)
    {
        if (!hasTriggeredHitEffect)
        {
            //공격이 적중했을 때
            hasTriggeredHitEffect = true;
            addEmitCount();
        }
    }

    public void ExecuteAttackGrab()
    {
        if (!canAttackGrap || activeSweepType != null || grabAttackRoutine != null)
            return;

        longSweepInputDeadline = float.NegativeInfinity;
        longSweepQueued = false;
        canAttackSweep = false;
        canAttackGrap = false;
        state.IsAttackingGrab = true;
        hasTriggeredHitEffect = false;
        Player.Movement.flippable = false;

        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        grabAttackHitbox.OnTargetGrapHit += handleGrapHit;
        playerAnimation.PlayGrab();
        grabAttackRoutine = StartCoroutine(GrabAttackRoutine());
    }

    private void handleGrapHit(Transform target, Vector2 hitPoint)
    {
        currentGrabTarget = target.GetComponent<IGrabbable>();

        if (currentGrabTarget != null)
        {
            currentTargetTransform = target;
            if (!hasTriggeredHitEffect)
            {
                //공격이 적중했을 때
                hasTriggeredHitEffect = true;
                addEmitCount();
            }
        }
    }

    private IEnumerator GrabAttackRoutine()
    {
        yield return new WaitForSeconds(grabHitTime);

        grabAttackHitbox.CatchClosestEnemy();
        StartGrabVisual();
        SoundManager.instance.PlaySFX(Player.Instance.grabThrowSound1);
        SoundManager.instance.PlaySFX(Player.Instance.grabThrowSound2);

        float resolveDelay = Mathf.Max(0f, grabResolveTime - grabHitTime);
        yield return new WaitForSeconds(resolveDelay);

        ResolveGrab();

        float recoveryDuration = Mathf.Max(0f, grabAnimationDuration - grabResolveTime);
        yield return new WaitForSeconds(recoveryDuration);

        grabAttackRoutine = null;
        FinishGrabAttack();
    }

    private void ResolveGrab()
    {
        if (currentGrabTarget != null)
            currentGrabTarget.ReceiveGrab();
        else if (currentTargetTransform == null)
            grapEffect.StartGrabFailVisualRetract();

        canAttackSweep = true;
        SoundManager.instance.PlaySFX(Player.Instance.grabRetreiveSound);
    }

    private void FinishGrabAttack()
    {
        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        ClearGrabTarget();

        state.IsAttackingGrab = false;
        Player.Movement.flippable = true;

        canAttackSweep = true;
        canAttackGrap = true;
    }

    private void StartGrabVisual()
    {
        if (currentTargetTransform != null)
            grapEffect.StartGrabVisual(currentTargetTransform);
        else
            grapEffect.StartGrabFailVisual();
    }

    private void ClearGrabTarget()
    {
        currentTargetTransform = null;
        currentGrabTarget = null;
        grapEffect.StopGrabVisual();
    }

    private void CancelGrabAttack()
    {
        if (grabAttackRoutine != null)
        {
            StopCoroutine(grabAttackRoutine);
            grabAttackRoutine = null;
        }

        if (grabAttackHitbox != null)
            grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;

        ClearGrabTarget();
        state.IsAttackingGrab = false;
        Player.Movement.flippable = true;
        canAttackSweep = true;
        canAttackGrap = true;
    }

    private void CancelAllAttacks()
    {
        if (sweepAttackRoutine != null)
        {
            StopCoroutine(sweepAttackRoutine);
            sweepAttackRoutine = null;
        }

        if (sweepCooldownRoutine != null)
        {
            StopCoroutine(sweepCooldownRoutine);
            sweepCooldownRoutine = null;
        }

        if (grabAttackRoutine != null)
        {
            StopCoroutine(grabAttackRoutine);
            grabAttackRoutine = null;
        }

        CloseSweepHitbox();

        if (grabAttackHitbox != null)
            grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;

        if (grapEffect != null)
            ClearGrabTarget();

        if (state != null)
        {
            state.IsAttackingSweep = false;
            state.IsAttackingGrab = false;
        }

        if (Player.Movement != null)
            Player.Movement.flippable = true;

        activeSweepType = null;
        longSweepQueued = false;
        longSweepInputDeadline = float.NegativeInfinity;
        canAttackSweep = true;
        canAttackGrap = true;
    }

    private void OnEmitPressed()
    {
        currentEmitRecoveryCount = 0;
        setEmitableUI(currentEmitRecoveryCount);
        hasTriggeredEmit = false;
    }

    private void addEmitCount()
    {
        if (currentEmitRecoveryCount >= emitRecoveryCount)
        {
            EmitManager.Instance.isEmittable = true;
        }
        else
        {
            setEmitableUI(currentEmitRecoveryCount);
            currentEmitRecoveryCount++;
        }
    }

    //역행 카운트 계산 및 검사
    private void CheckEmitable()
    {
        //역행 가능상태가 되었을 시 한번만 동작
        if(EmitManager.Instance.isEmittable && !hasTriggeredEmit)
        {
            setEmitableUI(currentEmitRecoveryCount);
            SoundManager.instance.PlaySFX(Player.Instance.canEmitSound);
            hasTriggeredEmit = true;
        }
    }

    private void setEmitableUI(int count)
    {
        EventManager.TriggerEvent<int>("OnEmitRecoveryCount", count);
    }
}

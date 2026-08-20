using System.Collections;
using UnityEngine;
/*
 * 플레이어 공격, 역행 관리용 클래스
 */
public class PlayerCombat : MonoBehaviour
{
    private PlayerInputHandler input;
    private PlayerState state;
    private GrapEffect grapEffect;
    private EmitManager emitManager;

    [Header("Hitbox Colliders")]
    public PlayerAttackHitbox sweepAttackHitbox;
    public PlayerGrapHitbox grabAttackHitbox;

    [Header("Attack Settings")]
    public float attackCoolTime = 0.3f;
    public float attackRecoveryTime = 1f;
    public int maxAttackCount = 2;
    public int currentAttackCount;
    private Coroutine attackRecoveryRoutine;

    [Header("Emit Settings")]
    const float emitRecoveryAmountWhenAttackHit = 0.5f;
    private bool hasTriggeredEmit;

    private bool canAttackSweep = true;
    private bool canAttackGrap = true;
    private bool hasTriggeredHitEffect = false;

    private IGrabbable currentGrabTarget;

    private Transform currentTargetTransform;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        state = Player.State;
        grapEffect = Player.GrapEffect;

        currentAttackCount = maxAttackCount;
    }

    private void Start()
    {
        emitManager = EmitManager.Instance;
    }

    private void OnEnable()
    {
        EventManager.StartListening("OnEmitPressed", OnEmitPressed);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnEmitPressed", OnEmitPressed);
    }

    private void Update()
    {
        if (state.IsDead) return;
        CheckInputs();
        CheckEmitable();
        if (currentAttackCount < maxAttackCount && attackRecoveryRoutine == null)
        {
            attackRecoveryRoutine = StartCoroutine(AttackRecoveryRoutine());
        }
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
        if (currentAttackCount == 0) return;
        else if (canAttackSweep)
        {
            if(state.CurrentState == State.AttackingGrab)
            {
                AnimEvent_AttackGrapEnd();
            }
            canAttackSweep = false;
            canAttackGrap = false;
            state.IsAttackingSweep = true;
            hasTriggeredHitEffect = false;

            currentAttackCount--;
            Player.UIController.UpdateAttackBar();
        }
    }

    public void AnimEvent_AttackSweepStart()
    {
        Player.Movement.flippable = false;
        sweepAttackHitbox.OnTargetSweepHit += handleSweepHit;
        sweepAttackHitbox.gameObject.SetActive(true);
        SoundManager.instance.PlaySFX(Player.Instance.sweepSound);
    }

    public void AnimEvent_AttackSweepEnd()
    {
        Player.Movement.flippable = true;
        sweepAttackHitbox.gameObject.SetActive(false);
        sweepAttackHitbox.OnTargetSweepHit -= handleSweepHit;
        StartCoroutine(AttackSweepCooldowmRoutine());
    }

    private IEnumerator AttackSweepCooldowmRoutine()
    {
        state.IsAttackingSweep = false;
        canAttackGrap = true;

        yield return new WaitForSeconds(attackCoolTime);
        canAttackSweep = true;
    }

    private void handleSweepHit(Transform target, Vector2 hitPoint)
    {
        if (!hasTriggeredHitEffect && !emitManager.isEmitting)
        {
            //공격이 적중했을 떄
            hasTriggeredHitEffect = true;
            emitManager.AddEmitTime(emitRecoveryAmountWhenAttackHit);
        }
    }

    public void ExecuteAttackGrab()
    {
        if (currentAttackCount == 0) return;
        else if (canAttackGrap)
        {
            canAttackSweep = false;
            canAttackGrap = false;
            state.IsAttackingGrab = true;
            hasTriggeredHitEffect = false;
            Player.Movement.flippable = false;

            currentAttackCount--;
            Player.UIController.UpdateAttackBar();

            grabAttackHitbox.OnTargetGrapHit += handleGrapHit;
        }
    }

    private void handleGrapHit(Transform target, Vector2 hitPoint)
    {
        currentGrabTarget = target.GetComponent<IGrabbable>();

        if(currentGrabTarget !=null)
        {
            currentTargetTransform = target;
            if (!hasTriggeredHitEffect && !emitManager.isEmitting)
            {
                //공격이 적중했을 때
                hasTriggeredHitEffect = true;
                emitManager.AddEmitTime(emitRecoveryAmountWhenAttackHit);
            }
        }
    }

    public void AnimEvent_AttackGrapEnd()
    {
        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        currentTargetTransform = null;
        currentGrabTarget = null;
        grapEffect.StopGrabVisual();

        state.IsAttackingGrab = false;
        Player.Movement.flippable = true;

        canAttackSweep = true;
        canAttackGrap = true;
    }

    public void AnimEvent_GrabHitboxActive()
    {
        grabAttackHitbox.CatchClosestEnemy();
        startGrabVisual();
        SoundManager.instance.PlaySFX(Player.Instance.grabThrowSound1);
        SoundManager.instance.PlaySFX(Player.Instance.grabThrowSound2);
    }

    private void startGrabVisual()
    {
        //bool isTargetAlive = currentGrabTarget == null
        //    || currentGrabTarget.CurrentState != currentGrabTarget.StateMachine.DeadState;

        if (currentTargetTransform != null)
            grapEffect.StartGrabVisual(currentTargetTransform);
        else grapEffect.StartGrabFailVisual();
    }

    public void AnimEvent_startEnemyGrab()
    {
        if (currentGrabTarget != null)
            currentGrabTarget.ReceiveGrab();
        else if (currentTargetTransform == null)
        {
            grapEffect.StartGrabFailVisualRetract();
        }
        canAttackSweep = true;
        SoundManager.instance.PlaySFX(Player.Instance.grabRetreiveSound);
    }

    private IEnumerator AttackRecoveryRoutine()
    {
        yield return new WaitForSeconds(attackRecoveryTime);
        currentAttackCount++;
        Player.UIController.UpdateAttackBar();
        attackRecoveryRoutine = null;
    }

    private void StopAttackRecoveryRoutine()
    {
        StopCoroutine(attackRecoveryRoutine);
        attackRecoveryRoutine = null;
    }

    private void OnEmitPressed()
    {
        if(currentAttackCount < maxAttackCount)
            currentAttackCount++;

        if (currentAttackCount == maxAttackCount && attackRecoveryRoutine != null)
            StopAttackRecoveryRoutine();

        hasTriggeredEmit = false;
    }

    private void CheckEmitable()
    {
        //역행 가능상태가 되었을 시 한번만 동작
        if(emitManager.isEmittable && !hasTriggeredEmit)
        {
            SoundManager.instance.PlaySFX(Player.Instance.canEmitSound);
            hasTriggeredEmit = true;
        }
    }
}

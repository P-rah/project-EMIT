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
    //public int maxAttackCount = 2;
    //public int currentAttackCount;
    private Coroutine attackRecoveryRoutine;

    [Header("Emit Settings")]
    const float emitRecoveryAmountWhenAttackHit = 0.5f;
    private bool hasTriggeredEmit;

    private bool canAttackSweep = true;
    private bool canAttackGrap = true;
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
        if (canAttackSweep)
        {
            if(state.CurrentState == State.AttackingGrab)
            {
                AnimEvent_AttackGrapEnd();
            }
            canAttackSweep = false;
            canAttackGrap = false;
            IsAttackingSweep = true;
            hasTriggeredHitEffect = false;
        }
    }

    public void AnimEvent_AttackSweepStart()
    {
        Player.Movement.flippable = false;
        Player.TempMovement.flippable = false;
        sweepAttackHitbox.OnTargetSweepHit += handleSweepHit;
        sweepAttackHitbox.gameObject.SetActive(true);
        SoundManager.Instance.PlaySFX(Player.Instance.sweepSound);
    }

    public void AnimEvent_AttackSweepEnd()
    {
        Player.Movement.flippable = true;
        Player.TempMovement.flippable = true;
        sweepAttackHitbox.gameObject.SetActive(false);
        sweepAttackHitbox.OnTargetSweepHit -= handleSweepHit;
        StartCoroutine(AttackSweepCooldowmRoutine());
    }

    private IEnumerator AttackSweepCooldowmRoutine()
    {
        IsAttackingSweep = false;
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
        if (canAttackGrap)
        {
            canAttackSweep = false;
            canAttackGrap = false;
            IsAttackingGrab = true;
            hasTriggeredHitEffect = false;
            Player.Movement.flippable = false;
            Player.TempMovement.flippable = false;

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

        IsAttackingGrab = false;
        Player.Movement.flippable = true;
        Player.TempMovement.flippable = true;

        canAttackSweep = true;
        canAttackGrap = true;
    }

    public void AnimEvent_GrabHitboxActive()
    {
        grabAttackHitbox.CatchClosestEnemy();
        startGrabVisual();
        SoundManager.Instance.PlaySFX(Player.Instance.grabThrowSound1);
        SoundManager.Instance.PlaySFX(Player.Instance.grabThrowSound2);
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
        SoundManager.Instance.PlaySFX(Player.Instance.grabRetreiveSound);
    }

    private void OnEmitPressed()
    {
        hasTriggeredEmit = false;
    }

    private void CheckEmitable()
    {
        //역행 가능상태가 되었을 시 한번만 동작
        if(emitManager.isEmittable && !hasTriggeredEmit)
        {
            SoundManager.Instance.PlaySFX(Player.Instance.canEmitSound);
            hasTriggeredEmit = true;
        }
    }
}

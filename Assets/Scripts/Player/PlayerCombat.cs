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

    [Header("Hitbox Colliders")]
    public PlayerAttackHitbox sweepAttackHitbox;
    public PlayerGrapHitbox grabAttackHitbox;

    [Header("Attack Settings")]
    public float attackCoolTime = 0.3f;
    public float attackRecoveryTime = 1f;
    public int maxAttackCount = 2;
    private int currentAttackCount;
    private Coroutine attackRecoveryRoutine;

    [Header("Emit Settings")]
    public int emitRecoveryCount = 5;
    public int currentEmitRecoveryCount = 0;

    private bool canAttackSweep = true;
    private bool canAttackGrap = true;
    private bool hasTriggeredHitEffect = false;

    private Enemy currentTarget;
    private Transform currentTargetTransform;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        state = Player.State;
        grapEffect = Player.GrapEffect;

        currentAttackCount = maxAttackCount;
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
        if (Player.State.isDeath) return;
        CheckInputs();
        if(currentAttackCount < maxAttackCount && attackRecoveryRoutine == null)
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
        if (!hasTriggeredHitEffect)
        {
            //공격이 적중했을 떄
            hasTriggeredHitEffect = true;
            EmitRecoveryCalculate();
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

            grabAttackHitbox.OnTargetGrapHit += handleGrapHit;
        }
    }

    private void handleGrapHit(Transform target, Vector2 hitPoint)
    {
        currentTarget = target.GetComponent<Enemy>();
        currentTargetTransform = target;
        if (!hasTriggeredHitEffect)
        {
            //공격이 적중했을 때
            hasTriggeredHitEffect = true;
            EmitRecoveryCalculate();
        }
    }

    public void AnimEvent_AttackGrapEnd()
    {
        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        currentTargetTransform = null;
        currentTarget = null;
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
        if(currentTargetTransform != null && currentTarget.currentState != currentTarget.DeadState)
            grapEffect.StartGrabVisual(currentTargetTransform);
        else grapEffect.StartGrabFailVisual();
    }

    public void AnimEvent_startEnemyGrab()
    {
        if (currentTarget != null && currentTarget.currentState != currentTarget.DeadState)
            currentTarget.ReceiveGrab();
        else
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

        currentEmitRecoveryCount = 0;
    }

    private void EmitRecoveryCalculate()
    {
        currentEmitRecoveryCount++;
        if (currentEmitRecoveryCount >= emitRecoveryCount)
        {
            currentEmitRecoveryCount = 0;
            EmitManager.Instance.isEmittable = true;
        }
    }
}

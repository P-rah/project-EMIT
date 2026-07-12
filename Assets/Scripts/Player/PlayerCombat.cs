using System.Collections;
using UnityEngine;
/*
 * 플레이어 공격, 역행 관리용 클래스
 */
public class PlayerCombat : MonoBehaviour
{
    private PlayerState state;
    private GrapEffect grapEffect;

    [Header("Hitbox Colliders")]
    public PlayerAttackHitbox sweepAttackHitbox;
    public PlayerGrapHitbox grabAttackHitbox;

    [Header("Attack Settings")]
    public float attackingSweepTime = 0.2f;
    public float attackingGrabTime = 0.5f;
    public float attackCoolTime = 0.3f;

    private bool hasTriggeredHitEffect = false;

    [Header("Emit Settings")]
    public float emitCooldown = 1f;

    private bool canAttackSweep = true;
    private bool canAttackGrap = true;
    private bool canEmit = true;

    private Enemy currentTarget;
    private Transform currentTargetTransform;

    private void Awake()
    {
        state = Player.State;
        grapEffect = Player.GrapEffect;
    }

    private void OnEnable()
    {
        EventManager.StartListening("OnEmitPressed", ExecuteEmit);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnEmitPressed", ExecuteEmit);
    }

    public void ExecuteAttackSweep()
    {
        if (canAttackSweep)
        {
            if(state.CurrentState == State.AttackingGrab)
            {
                AnimEvent_stopGrabVisual();
                AnimEvent_AttackGrapEnd();
            }
            StartCoroutine(AttackSweepRoutine());
        }
    }

    public void ExecuteAttackGrab()
    {
        if (canAttackGrap)
        {
            canAttackSweep = false;
            canAttackGrap = false;
            state.isAttackingGrab = true;
            hasTriggeredHitEffect = false;

            grabAttackHitbox.OnTargetGrapHit += handleGrapHit;
        }
    }

    public void ExecuteEmit()
    {
        if (canEmit)
            StartCoroutine(EmitRoutine());
    }

    private IEnumerator AttackSweepRoutine()
    {
        canAttackSweep = false;
        canAttackGrap = false;
        state.isAttackingSweep = true;
        hasTriggeredHitEffect = false;

        sweepAttackHitbox.OnTargetSweepHit += handleSweepHit;
        sweepAttackHitbox.gameObject.SetActive(true);

        yield return new WaitForSeconds(attackingSweepTime);

        sweepAttackHitbox.gameObject.SetActive(false);
        sweepAttackHitbox.OnTargetSweepHit -= handleSweepHit;

        state.isAttackingSweep = false;

        yield return new WaitForSeconds(attackCoolTime);
        canAttackSweep = true;
        canAttackGrap = true;
    }

    private void handleSweepHit(Transform target, Vector2 hitPoint)
    {
        if (!hasTriggeredHitEffect)
        {
            CameraShake.Instance.ShakeCamera(10f, 0.2f);
            hasTriggeredHitEffect = true;
        }
    }


    private void handleGrapHit(Transform target, Vector2 hitPoint)
    {
        currentTarget = target.GetComponent<Enemy>();
        currentTargetTransform = target;
        if (!hasTriggeredHitEffect)
        {
            CameraShake.Instance.ShakeCamera(10f, 0.2f);
            hasTriggeredHitEffect = true;
        }
    }

    public void AnimEvent_AttackGrapEnd()
    {
        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        currentTargetTransform = null;

        state.isAttackingGrab = false;

        canAttackSweep = true;
        canAttackGrap = true;
    }

    public void AnimEvent_GrabHitboxActive()
    {
        grabAttackHitbox.CatchClosestEnemy();
    }

    public void AnimEvent_startGrabVisual()
    {
        if(currentTargetTransform != null)
            grapEffect.StartGrabVisual(currentTargetTransform);
    }

    public void AnimEvent_stopGrabVisual()
    {
        grapEffect.StopGrabVisual();
    }

    public void AnimEvent_startEnemyGrab()
    {
        if (currentTarget != null)
            currentTarget.ReceiveGrab();
        canAttackSweep = true;
    }

    private IEnumerator EmitRoutine()
    {
        canEmit= false;
        state.isEmiting = !state.isEmiting;

        yield return new WaitForSeconds(emitCooldown);
        canEmit = true;
    }
}

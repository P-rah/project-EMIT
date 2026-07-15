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
    private bool canAttackSweep = true;
    private bool canAttackGrap = true;

    private Enemy currentTarget;
    private Transform currentTargetTransform;

    private void Awake()
    {
        state = Player.State;
        grapEffect = Player.GrapEffect;
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
            canAttackSweep = false;
            canAttackGrap = false;
            state.isAttackingSweep = true;
            hasTriggeredHitEffect = false;
        }
    }

    public void AnimEvent_AttackSweepStart()
    {
        Player.Movement.flipable = false;
        sweepAttackHitbox.OnTargetSweepHit += handleSweepHit;
        sweepAttackHitbox.gameObject.SetActive(true);
    }

    public void AnimEvent_AttackSweepEnd()
    {
        Player.Movement.flipable = true;
        sweepAttackHitbox.gameObject.SetActive(false);
        sweepAttackHitbox.OnTargetSweepHit -= handleSweepHit;
        StartCoroutine(AttackSweepCooldowmRoutine());
    }

    private IEnumerator AttackSweepCooldowmRoutine()
    {
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
        if (EmitManager.Instance.isEmitting)
        {
            EventManager.TriggerEvent("OnEmitAborted");
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
            Player.Movement.flipable = false;

            grabAttackHitbox.OnTargetGrapHit += handleGrapHit;
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
        if (EmitManager.Instance.isEmitting)
        {
            EventManager.TriggerEvent("OnEmitAborted");
        }
    }

    public void AnimEvent_AttackGrapEnd()
    {
        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        currentTargetTransform = null;

        state.isAttackingGrab = false;
        Player.Movement.flipable = true;

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
}

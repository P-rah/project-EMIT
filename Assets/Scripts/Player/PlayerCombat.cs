using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private bool hasTriggeredHitEffect = false;

    [Header("Emit Settings")]
    private bool canAttackSweep = true;
    private bool canAttackGrap = true;

    private Enemy currentTarget;
    private Transform currentTargetTransform;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        state = Player.State;
        grapEffect = Player.GrapEffect;
    }

    private void Update()
    {
        CheckInputs();
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
            state.isAttackingSweep = true;
            hasTriggeredHitEffect = false;
        }
    }

    public void AnimEvent_AttackSweepStart()
    {
        Player.Movement.flippable = false;
        sweepAttackHitbox.OnTargetSweepHit += handleSweepHit;
        sweepAttackHitbox.gameObject.SetActive(true);
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

    public void ExecuteAttackGrab()
    {
        if (canAttackGrap)
        {
            canAttackSweep = false;
            canAttackGrap = false;
            state.isAttackingGrab = true;
            hasTriggeredHitEffect = false;
            Player.Movement.flippable = false;

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
    }

    public void AnimEvent_AttackGrapEnd()
    {
        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        currentTargetTransform = null;
        currentTarget = null;
        grapEffect.StopGrabVisual();

        state.isAttackingGrab = false;
        Player.Movement.flippable = true;

        canAttackSweep = true;
        canAttackGrap = true;
    }

    public void AnimEvent_GrabHitboxActive()
    {
        grabAttackHitbox.CatchClosestEnemy();
        startGrabVisual();
    }

    private void startGrabVisual()
    {
        if(currentTargetTransform != null)
            grapEffect.StartGrabVisual(currentTargetTransform);
        else grapEffect.StartGrabFailVisual();
    }

    public void AnimEvent_startEnemyGrab()
    {
        if (currentTarget != null)
            currentTarget.ReceiveGrab();
        else
        {
            grapEffect.StartGrabFailVisualRetract();

        }
        canAttackSweep = true;
    }
}

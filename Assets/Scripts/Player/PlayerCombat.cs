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
    public PlayerAttackHitbox grabAttackHitbox;

    [Header("Attack Settings")]
    public float attackingSweepTime = 0.2f;
    public float attackingGrabTime = 0.5f;
    public float attackCoolTime = 0.3f;

    private bool hasTriggeredHitEffect = false;

    [Header("Emit Settings")]
    public float emitCooldown = 1f;

    private bool canAttack = true;
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
        if (canAttack)
            StartCoroutine(AttackSweepRoutine());
    }

    public void ExecuteAttackGrab()
    {
        if (canAttack)
            StartCoroutine(AttackGrabRoutine());
    }

    public void ExecuteEmit()
    {
        if (canEmit)
            StartCoroutine(EmitRoutine());
    }

    private IEnumerator AttackSweepRoutine()
    {
        canAttack = false;
        state.isAttackingSweep = true;
        hasTriggeredHitEffect = false;

        sweepAttackHitbox.OnTargetHit += handleSweepHit;
        sweepAttackHitbox.gameObject.SetActive(true);

        yield return new WaitForSeconds(attackingSweepTime);

        sweepAttackHitbox.gameObject.SetActive(false);
        sweepAttackHitbox.OnTargetHit -= handleSweepHit;

        state.isAttackingSweep = false;

        yield return new WaitForSeconds(attackCoolTime);
        canAttack = true;
    }

    private void handleSweepHit(Transform target, Vector2 hitPoint)
    {

        if (!hasTriggeredHitEffect)
        {
            CameraShake.Instance.ShakeCamera(10f, 0.2f);
            hasTriggeredHitEffect = true;
        }
    }


    private IEnumerator AttackGrabRoutine()
    {
        canAttack = false;
        state.isAttackingGrab = true;
        hasTriggeredHitEffect = false;

        grabAttackHitbox.OnTargetHit += handleGrabHit;
        grabAttackHitbox.gameObject.SetActive(true);

        yield return new WaitForSeconds(attackingGrabTime);

        grabAttackHitbox.gameObject.SetActive(false);
        grabAttackHitbox.OnTargetHit -= handleGrabHit;
        currentTargetTransform = null;

        state.isAttackingGrab = false;

        yield return new WaitForSeconds(attackCoolTime);
        canAttack = true;
    }

    private void handleGrabHit(Transform target, Vector2 hitPoint)
    {
        currentTarget = target.GetComponent<Enemy>();
        currentTargetTransform = target;
        if (!hasTriggeredHitEffect)
        {
            CameraShake.Instance.ShakeCamera(10f, 0.2f);
            hasTriggeredHitEffect = true;
        }
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
    }

    private IEnumerator EmitRoutine()
    {
        canEmit= false;
        state.isEmiting = !state.isEmiting;

        yield return new WaitForSeconds(emitCooldown);
        canEmit = true;
    }
}

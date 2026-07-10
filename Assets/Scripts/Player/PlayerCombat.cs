using System.Collections;
using UnityEngine;
/*
 * 플레이어 공격, 역행 관리용 클래스
 */
public class PlayerCombat : MonoBehaviour
{
    private PlayerState state;

    [Header("Hitbox Colliders")]
    public GameObject sweepCollider;
    public GameObject grabCollider;

    [Header("Attack Settings")]
    public float attackingSweepTime = 0.2f;
    public float attackingGrabTime = 0.5f;
    public float attackCoolTime = 0.3f;

    [Header("Emit Settings")]
    public float emitCooldown = 1f;

    private bool canAttack = true;
    private bool canEmit = true;

    private void Awake()
    {
        state = Player.State;
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

        //���� ó��
        sweepCollider.SetActive(true);

        yield return new WaitForSeconds(attackingSweepTime);
        sweepCollider.SetActive(false);
        state.isAttackingSweep = false;

        yield return new WaitForSeconds(attackCoolTime);
        canAttack = true;
    }

    private IEnumerator AttackGrabRoutine()
    {
        canAttack = false;
        state.isAttackingGrab = true;

        //���� ó��
        grabCollider.SetActive(true);

        yield return new WaitForSeconds(attackingGrabTime);
        grabCollider.SetActive(false);
        state.isAttackingGrab = false;

        yield return new WaitForSeconds(attackCoolTime);
        canAttack = true;
    }

    private IEnumerator EmitRoutine()
    {
        canEmit= false;
        state.isEmiting = !state.isEmiting;

        yield return new WaitForSeconds(emitCooldown);
        canEmit = true;
    }
}

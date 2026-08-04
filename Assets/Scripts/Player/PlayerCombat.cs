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

    private bool hasTriggeredHitEffect = false;

    [Header("Emit Settings")]
    private bool canAttackSweep = true;
    private bool canAttackGrap = true;

    /* INTEGRATION:
     * 이 브랜치에서 Boss1은 Enemy를 상속하지 않고 IDamageable을 직접 구현하므로 일반 적과 보스 참조를
     * 따로 보관한다. integration/enemy-state-merge의 공격 횟수/역행 회복/UI 로직은 그대로 가져오되,
     * 이 세 타깃 필드와 handleGrapHit/정리/그랩 분기까지 함께 유지해야 보스 그랩이 누락되지 않는다.
     */
    private Enemy currentTarget;
    private Boss1 currentBossTarget;
    private Transform currentTargetTransform;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        state = Player.State;
        grapEffect = Player.GrapEffect;
    }

    private void Update()
    {
        if (Player.State.isDeath) return;
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
            state.IsAttackingSweep = true;
            hasTriggeredHitEffect = false;
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
        }
    }

    public void ExecuteAttackGrab()
    {
        if (canAttackGrap)
        {
            canAttackSweep = false;
            canAttackGrap = false;
            state.IsAttackingGrab = true;
            hasTriggeredHitEffect = false;
            Player.Movement.flippable = false;

            grabAttackHitbox.OnTargetGrapHit += handleGrapHit;
        }
    }

    private void handleGrapHit(Transform target, Vector2 hitPoint)
    {
        currentTarget = target.GetComponent<Enemy>();
        currentBossTarget = target.GetComponent<Boss1>();
        currentTargetTransform = target;
        if (!hasTriggeredHitEffect)
        {
            //공격이 적중했을 때
            hasTriggeredHitEffect = true;
        }
    }

    public void AnimEvent_AttackGrapEnd()
    {
        grabAttackHitbox.OnTargetGrapHit -= handleGrapHit;
        currentTargetTransform = null;
        currentTarget = null;
        currentBossTarget = null;
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
        /* INTEGRATION:
         * currentTarget이 null이어도 currentTargetTransform이 있으면 Boss1을 맞힌 경우이므로 살아 있는
         * 대상으로 취급한다. 일반 적의 사망 여부는 Enemy.currentState가 아니라 StateMachine.CurrentState로
         * 확인한다. 통합 브랜치의 기존 조건식으로 되돌리면 보스에서 null 참조가 발생하고 상태 머신의
         * 소유권도 우회한다. 향후 Boss1에 명시적인 사망 상태가 생기면 이 조건에 별도 검사만 추가한다.
         */
        bool isTargetAlive = currentTarget == null
            || currentTarget.CurrentState != currentTarget.StateMachine.DeadState;

        if (currentTargetTransform != null && isTargetAlive)
            grapEffect.StartGrabVisual(currentTargetTransform);
        else grapEffect.StartGrabFailVisual();
    }

    public void AnimEvent_startEnemyGrab()
    {
        /* INTEGRATION:
         * 일반 적은 EnemyStateMachine의 DeadState를 확인한 뒤 ReceiveGrab을 호출하고, 일반 적이 아니면
         * Boss1 전용 ReceiveGrab으로 넘긴다. 실패 연출은 어떤 IDamageable도 맞히지 못해 Transform 자체가
         * 없을 때만 실행해야 하므로 currentTargetTransform == null 조건을 유지한다.
         */
        if (currentTarget != null && currentTarget.CurrentState != currentTarget.StateMachine.DeadState)
            currentTarget.ReceiveGrab();
        else if (currentBossTarget != null)
            currentBossTarget.ReceiveGrab();
        else if (currentTargetTransform == null)
        {
            grapEffect.StartGrabFailVisualRetract();
        }
        canAttackSweep = true;
        SoundManager.instance.PlaySFX(Player.Instance.grabRetreiveSound);
    }
}

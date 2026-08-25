using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
/*
 * 플레이어 공격, 역행 관리용 클래스
 */
public class PlayerCombat : MonoBehaviour
{
    private PlayerAnimation animation;
    private PlayerInputHandler input;
    private PlayerState state;
    private GrapEffect grapEffect;
    private EmitManager emitManager;
    private TempPlayerMovement playerMovement;

    [Header("Hitbox Colliders")]
    public PlayerAttackHitbox[] sweepAttackHitbox;
    public PlayerGrapHitbox grabAttackHitbox;

    [Header("Attack Settings")]
    [SerializeField] private int maxSweepCount = 3;
    public int sweepCount;
    private Coroutine continuousSweepTimer;

    [Header("Emit Settings")]
    const float emitRecoveryAmountWhenAttackHit = 0.5f;
    private bool hasTriggeredEmit;

    private bool canAttack = true;
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
        animation = Player.Animation;
        sweepCount = 0;
        playerMovement = Player.TempMovement;
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
        if (canAttack)
        {
            if (state.CurrentState == State.AttackingGrab)
            {
                AnimEvent_AttackGrapEnd();
            }
            canAttack = false;
            IsAttackingSweep = true;
            hasTriggeredHitEffect = false;

            if (sweepCount >= maxSweepCount) sweepCount = 1;
            else sweepCount++;
            animation.SetAttackSweep(sweepCount);
        }
        else Debug.Log("공격불가");
    }

    public void PlayComboEffect(int comboIndex)
    {
        if (comboIndex < 0 || comboIndex >= sweepAttackHitbox.Length) return;

        Debug.Log("시작 : " + comboIndex);
        sweepAttackHitbox[comboIndex].gameObject.SetActive(true);
        sweepAttackHitbox[comboIndex].OnTargetSweepHit += handleSweepHit;
    }

    public void EndComboEffect(int comboIndex)
    {
        if (comboIndex < 0 || comboIndex >= sweepAttackHitbox.Length) return;

        Debug.Log("끝 : " + comboIndex);
        sweepAttackHitbox[comboIndex].gameObject.SetActive(false);
        sweepAttackHitbox[comboIndex].OnTargetSweepHit -= handleSweepHit;
    }

    public void AnimEvent_AttackSweepStart()
    {
        playerMovement.flippable = false;
        PlayComboEffect(sweepCount - 1);
        SoundManager.instance.PlaySFX(Player.Instance.sweepSound);
        playerMovement.TriggerMoveWhileAttack();
    }

    public void AnimEvent_AttackSweepEnd()
    {
        playerMovement.flippable = true;
        EndComboEffect(sweepCount - 1);
        //IsAttackingSweep = false;
        canAttack = true;
        IsAttackingSweep = false;
    }

    public void AnimEvent_ResetSweep()
    {
        animation.ResetAttackSweep();
        //IsAttackingSweep = false;
        canAttack = true;
        if (continuousSweepTimer != null)
        {
            StopCoroutine(continuousSweepTimer);
            continuousSweepTimer = null;
        }

        continuousSweepTimer = StartCoroutine(ResetSweepCount());
    }

    private IEnumerator ResetSweepCount()
    {
        yield return new WaitForFixedUpdate();
        sweepCount = 0;
        continuousSweepTimer = null;
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
        if (canAttack)
        {
            canAttack = false;
            IsAttackingGrab = true;
            hasTriggeredHitEffect = false;
            //Player.Movement.flippable = false;
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
        //Player.Movement.flippable = true;
        Player.TempMovement.flippable = true;

        canAttack = true;
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
        canAttack = true;
        SoundManager.instance.PlaySFX(Player.Instance.grabRetreiveSound);
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
            SoundManager.instance.PlaySFX(Player.Instance.canEmitSound);
            hasTriggeredEmit = true;
        }
    }
}

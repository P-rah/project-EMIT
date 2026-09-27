//using UnityEngine;

//public class BossAnimator : MonoBehaviour
//{
//    private Animator animator;

//    private readonly int hashIdle = Animator.StringToHash("Idle");
//    private readonly int hashRecoli = Animator.StringToHash("Recoli");
//    private readonly int hashGroggy = Animator.StringToHash("Groggy");
//    private readonly int hashAttack = Animator.StringToHash("Attack");

//    private void Awake()
//    {
//        animator = GetComponent<Animator>();
//        StartAnimControl();
//    }

//    public void StopAnimControl()
//    {
//        if (animator != null)
//        {
//            animator.enabled = false;
//        }
//    }

//    public void StartAnimControl()
//    {
//        if (animator != null)
//        {
//            animator.enabled = true;
//        }
//    }

//    internal void HandleStateEntered(Boss1 owner, StateBase<BossStateMachine> state)
//    {
//        if (owner == null || state == null || owner.StateMachine == null)
//        {
//            return;
//        }

//        BossStateMachine stateMachine = owner.StateMachine;

//        switch (state)
//        {
//            case var _ when state == stateMachine.MoveState:
//                PlayIdle();
//                break;

//            case var _ when state == stateMachine.RecoilState:
//                PlayRecoli();
//                break;

//            case var _ when state == stateMachine.GroggyState:
//                PlayGroggy();
//                break;

//            case var _ when state == stateMachine.KnockbackState:
//                PlayGroggy();
//                break;

//            case var _ when state == stateMachine.GrabbedState:
//                PlayGroggy();
//                break;

//            case var _ when state == stateMachine.AttackState:
//                PlayAttackStart();
//                break;

//        }
//    }

//    private void PlayIdle()
//    {
//        Play(hashIdle);
//    }

//    private void PlayRecoli()
//    {
//        Play(hashRecoli);
//    }

//    private void PlayGroggy()
//    {
//        Play(hashGroggy);
//    }

//    private void PlayAttackStart()
//    {
//        Play(hashAttack);
//    }

//    private void Play(int stateHash)
//    {
//        if (animator != null)
//        {
//            animator.Play(stateHash);
//        }
//    }
//}


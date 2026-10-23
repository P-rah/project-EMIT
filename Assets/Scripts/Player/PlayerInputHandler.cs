using System;
using UnityEngine;
using UnityEngine.InputSystem;
/*
 * 플레이어 입력을 받아 인풋을 처리하고 이벤트를 발생시키는 Handler 클래스
 */
public class PlayerInputHandler : MonoBehaviour
{

    private TutorialActionType? allowedAction = null;

    public event Action<TutorialActionType> OnActionPerformed;

    [Header("Optional Jump / Dash Buffer")]
    [SerializeField] private InputBuffer inputBuffer;
    [SerializeField] private bool useInputBuffer = true;

    private IBufferedCommand jumpCommand;
    private IBufferedCommand dashCommand;
    private PlayerInput playerInput;
    private PlayerState playerState;

    public InputBuffer Buffer => inputBuffer;
    public bool UsingInputBuffer => useInputBuffer && inputBuffer != null && inputBuffer.isActiveAndEnabled;
    public bool GameplayInputBlocked => !isActiveAndEnabled ||
        (playerInput != null && !playerInput.inputIsActive) ||
        (playerState != null && playerState.IsDead) ||
        (TimeManager.Instance != null && TimeManager.Instance.IsPaused) || Time.timeScale <= 0f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        if (UsingInputBuffer && GameplayInputBlocked) ResetAllInputs();
    }

    private void OnDisable()
    {
        ResetAllInputs();
    }

    public void SetInputBufferEnabled(bool enabled)
    {
        Vector2 heldMove = MoveInput;
        ResetAllInputs();
        useInputBuffer = enabled;
        MoveInput = heldMove;
    }

    /*
        ===============================================
        LAYER 1: 인풋을 이용하는 스크립트들이 참조할 input flag들
        =============================================== 
    */

    public Vector2 MoveInput { get; private set; }
    public bool JumpInput;
    public bool DownInput;
    public bool DashInput;
    public bool EmitInput { get; private set; }
    public bool AttackSweepInput { get; private set; }
    public bool AttackGrabInput { get; private set; }

    /*
        ===============================================
        LAYER 2: Send Messages가 인식할 Input을 받아오는 함수들

        FIX: 외부에서 Flag를 바로 이용하는 것이 아니라,
        
        PlayerInputHandler는 커맨드를 생성해서 Buffer에 등록.

        ===============================================
    */

    public void OnMove(InputValue value)
    {
        if (!IsAllowed(TutorialActionType.MoveLeft) && !IsAllowed(TutorialActionType.MoveRight))
        {
            MoveInput = Vector2.zero;
            return;
        }
        MoveInput = value.Get<Vector2>();

        if (MoveInput.x < -0.5f) OnActionPerformed?.Invoke(TutorialActionType.MoveLeft);
        else if (MoveInput.x > 0.5f) OnActionPerformed?.Invoke(TutorialActionType.MoveRight);
    }

    public void OnJump(InputValue value)
    {
        if (!IsAllowed(TutorialActionType.Jump)) return;
        if (UsingInputBuffer && GameplayInputBlocked) return;

        if (value.isPressed)
        {
            if (UsingInputBuffer)
            {
                jumpCommand ??= new BufferedJumpCommand(GetComponent<TempPlayerMovement>());
                inputBuffer.Register(Command.Jump, jumpCommand);
            }
            else JumpInput = true;
            OnActionPerformed?.Invoke(TutorialActionType.Jump);
        }
    }

    public void OnDown(InputValue value)
    {
        if (!IsAllowed(TutorialActionType.Down)) return;

        if (value.isPressed)
        {
            DownInput = true;
            OnActionPerformed?.Invoke(TutorialActionType.Down);
        }
    }

    public void OnDash(InputValue value)
    {
        if (!IsAllowed(TutorialActionType.Dash)) return;
        if (UsingInputBuffer && GameplayInputBlocked) return;

        if (value.isPressed)
        {
            if (UsingInputBuffer)
            {
                dashCommand ??= new BufferedDashCommand(GetComponent<TempPlayerMovement>());
                inputBuffer.Register(Command.Dash, dashCommand);
            }
            else DashInput = true;
            OnActionPerformed?.Invoke(TutorialActionType.Dash);
        }
    }

    public void OnEmit(InputValue value)
    {
        if (!IsAllowed(TutorialActionType.Emit)) return;

        if (value.isPressed)
        {
            if (EmitManager.Instance.isEmitting) EventManager.TriggerEvent("OnEmitEnd");
            else EventManager.TriggerEvent("OnEmitPressed");
        }
    }

    public void OnAttackSweep(InputValue value)
    {
        if (!IsAllowed(TutorialActionType.AttackSweep)) return;

        if (value.isPressed)
        {
            AttackSweepInput = true;
            OnActionPerformed?.Invoke(TutorialActionType.AttackSweep);
        }
    }

    public void OnAttackGrab(InputValue value)
    {
        if (!IsAllowed(TutorialActionType.AttackGrab)) return;

        if (value.isPressed)
        {
            AttackGrabInput = true;
            OnActionPerformed?.Invoke(TutorialActionType.AttackGrab);
        }
    }

    public void OnEscape(InputValue value)
    {
        if (value.isPressed)
        {
            
        }
    }
    
    /*
        ===============================================
        LAYER 3: 1회성 입력을 처리하는 함수들 (누를 때 시전)
        ===============================================
    */

    public bool ConsumeJump()
    {
        bool result = JumpInput;
        JumpInput = false;
        return result;
    }

    public bool ConsumeDash()
    {
        bool result = DashInput;
        DashInput = false;
        return result;
    }

    public bool ConsumeDown()
    {
        bool result = DownInput;
        DownInput = false;
        return result;
    }

    public bool ConsumeAttackSweep()
    {
        bool result = AttackSweepInput;
        AttackSweepInput = false;
        return result;
    }

    public bool ConsumeAttackGrab()
    {
        bool result = AttackGrabInput;
        AttackGrabInput = false;
        return result;
    }

    /*
        ===============================================
        입력 제한 제어 함수
        =============================================== 
    */
    public void SetAllowedAction(TutorialActionType action)
    {
        allowedAction = action;
        ResetAllInputs();
    }

    public void ClearAllowedAction()
    {
        allowedAction = null;
    }

    private bool IsAllowed(TutorialActionType action)
    {
        if (!allowedAction.HasValue) return true;

        return allowedAction.Value == action;
    }

    public void ResetAllInputs()
    {
        if (inputBuffer != null) inputBuffer.Clear();
        GetComponent<TempPlayerMovement>()?.ClearPendingJumpInput();
        MoveInput = Vector2.zero;
        JumpInput = false;
        DownInput = false;
        DashInput = false;
        EmitInput = false;
        AttackSweepInput = false;
        AttackGrabInput = false;
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;
/*
 * 플레이어 입력 제어용 클래스
 */
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool DashTriggered { get; private set; }

    public bool AttackSweepTriggered { get; private set; }
    public bool AttackGrapTriggered { get; private set; }
    public bool EmitTriggered { get; private set; }

    public event Action OnJumpPressed;
    public event Action OnDashPressed;
    public event Action OnAttackSweepPressed;
    public event Action OnAttackGrapPressed;
    public event Action OnEmitPressed;

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            OnJumpPressed?.Invoke();
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            OnDashPressed?.Invoke();
        }
    }

    public void OnEmit(InputValue value)
    {
        if (value.isPressed)
        {
            OnEmitPressed?.Invoke();
        }
    }

    public void OnAttackSweep(InputValue value)
    {
        if (value.isPressed)
        {
            OnAttackSweepPressed?.Invoke();
        }
    }

    public void OnAttackGrap(InputValue value)
    {
        if (value.isPressed)
        {
            OnAttackGrapPressed?.Invoke();
        }
    }

    public void ConsumeJump() => JumpTriggered = false;

    public void ConsumeDash() => DashTriggered = false;


}
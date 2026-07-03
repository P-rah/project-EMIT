using System;
using UnityEngine;
using UnityEngine.InputSystem;
/*
 * 플레이어 입력을 받아 이벤트를 발생시키는 Handler 클래스
 */
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool DashTriggered { get; private set; }

    public bool AttackSweepTriggered { get; private set; }
    public bool AttackGrapTriggered { get; private set; }
    public bool EmitTriggered { get; private set; }

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            EventManager.TriggerEvent("OnJumpPressed");
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            EventManager.TriggerEvent("OnDashPressed");
        }
    }

    public void OnEmit(InputValue value)
    {
        if (value.isPressed)
        {
            EventManager.TriggerEvent("OnEmitPressed");
        }
    }

    public void OnAttackSweep(InputValue value)
    {
        if (value.isPressed)
        {
            EventManager.TriggerEvent("OnAttackSweepPressed");
        }
    }

    public void OnAttackGrap(InputValue value)
    {
        if (value.isPressed)
        {
            EventManager.TriggerEvent("OnAttackGrapPressed");
        }
    }

    public void ConsumeJump() => JumpTriggered = false;

    public void ConsumeDash() => DashTriggered = false;


}
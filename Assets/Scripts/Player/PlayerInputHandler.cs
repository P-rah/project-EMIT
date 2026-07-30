using UnityEngine;
using UnityEngine.InputSystem;
/*
 * 플레이어 입력을 받아 인풋을 처리하고 이벤트를 발생시키는 Handler 클래스
 */
public class PlayerInputHandler : MonoBehaviour
{
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
        ===============================================
    */

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            JumpInput = true;
        }
    }

    public void OnDown(InputValue value)
    {
        if (value.isPressed)
        {
            DownInput = true;
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            DashInput = true;
        }
    }

    public void OnEmit(InputValue value)
    {
        if (value.isPressed)
        {
            if(EmitManager.Instance.isEmittable)
                EventManager.TriggerEvent("OnEmitPressed");
        }
    }

    public void OnAttackSweep(InputValue value)
    {
        if (value.isPressed)
        {
            AttackSweepInput = true;
        }
    }

    public void OnAttackGrab(InputValue value)
    {
        if (value.isPressed)
        {
            AttackGrabInput = true;
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
}
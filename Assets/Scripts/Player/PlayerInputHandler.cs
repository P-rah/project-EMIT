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

    public int JumpPressedFrame { get; private set; } = -1;
    public int DownPressedFrame { get; private set; } = -1;
    public int DashPressedFrame { get; private set; } = -1;
    public int AttackSweepPressedFrame { get; private set; } = -1;
    public int AttackGrabPressedFrame { get; private set; } = -1;

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
        if (!value.isPressed) return;

        JumpInput = true;
        JumpPressedFrame = Time.frameCount;
    }

    public void OnDown(InputValue value)
    {
        if (!value.isPressed) return;

        DownInput = true;
        DownPressedFrame = Time.frameCount;
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed) return;

        DashInput = true;
        DashPressedFrame = Time.frameCount;
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
        if (!value.isPressed) return;

        AttackSweepInput = true;
        AttackSweepPressedFrame = Time.frameCount;
    }

    public void OnAttackGrab(InputValue value)
    {
        if (!value.isPressed) return;

        AttackGrabInput = true;
        AttackGrabPressedFrame = Time.frameCount;
    }
    
    /*
        ===============================================
        LAYER 3: 1회성 입력을 처리하는 함수들 (누를 때 시전)
        ===============================================
    */

    /// <summary>
    /// 점프 인풋을 받았다면 true를 리턴하며, 인풋 bool은 false로 설정한다.
    /// </summary>
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

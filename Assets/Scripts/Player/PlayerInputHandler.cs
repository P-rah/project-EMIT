using System;
using UnityEngine;
using UnityEngine.InputSystem;
/*
 * 플레이어 입력을 받아 이벤트를 발생시키는 Handler 클래스
 */
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerMovement PlayerMovement;
    private PlayerCombat PlayerCombat;
    
    public Vector2 MoveInput { get; private set; }

    public bool EmitTriggered { get; private set; }

    public void Awake()
    {
        PlayerMovement = GetComponent<PlayerMovement>();
        PlayerCombat = GetComponent<PlayerCombat>();
    }

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            PlayerMovement.ExecuteJump();
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            PlayerMovement.ExecuteDash();
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
            PlayerCombat.ExecuteAttackSweep();
        }
    }

    public void OnAttackGrab(InputValue value)
    {
        if (value.isPressed)
        {
            PlayerCombat.ExecuteAttackGrab();
        }
    }
}
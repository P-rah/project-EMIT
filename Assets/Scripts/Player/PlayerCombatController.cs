using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * 플레이어 공격, 역행 담당 스크립트
 */

public class PlayerCombatController : MonoBehaviour
{
    private bool isEmiting = false;
    private bool emitable = true;

    [Header("Emit Settings")]
    public float emitCooldown = 1f;

    public EmitShaderController _emitShaderController;

    public void OnEmit(InputAction.CallbackContext context)
    {
        if (emitable)
        {
            StartCoroutine(EmitRoutine());
        }

    }

    private IEnumerator EmitRoutine()
    {
        emitable = false;
        isEmiting = !isEmiting;

        if (isEmiting)
        {
            _emitShaderController.StartEffect();
            Debug.Log("Start");
        }
        else
        {
            _emitShaderController.EndEffect();
            Debug.Log("End");
        }
        yield return new WaitForSeconds(emitCooldown);
        emitable = true;
    }
}

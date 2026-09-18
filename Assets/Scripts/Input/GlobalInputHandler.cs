using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionReference pauseAction;

    private void OnEnable()
    {
        pauseAction.action.performed += OnPause;
        pauseAction.action.Enable();
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPause;
        pauseAction.action.Disable();
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        SceneControlManager.Instance.HandleBackInput();
    }
}
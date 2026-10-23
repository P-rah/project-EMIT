using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalInputHandler : MonoBehaviour
{
    public static GlobalInputHandler Instance { get; private set; }

    [SerializeField] private InputActionReference pauseAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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

    public void SetGameplayInputEnabled(bool enabled)
    {
        foreach (PlayerInput playerInput in PlayerInput.all)
        {
            if (enabled)
            {
                if (!playerInput.inputIsActive) playerInput.ActivateInput();
            }
            else
            {
                playerInput.GetComponent<PlayerInputHandler>()?.ResetAllInputs();
                if (playerInput.inputIsActive) playerInput.DeactivateInput();
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

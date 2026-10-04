using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class ExitZone : MonoBehaviour
{
    private BoxCollider2D exitCollider;
    private StageManager stageManager;

    private void Awake()
    {
        exitCollider = GetComponent<BoxCollider2D>();
        exitCollider.isTrigger = false;
    }

    private void Start()
    {
        stageManager = StageManager.Instance;
        if (stageManager == null)
        {
            Debug.LogError("ExitZone requires an active StageManager.", this);
            enabled = false;
            return;
        }

        stageManager.StageCleared += EnableExit;

        if (stageManager.CanMoveToNextStage)
        {
            EnableExit();
        }
    }

    private void OnDestroy()
    {
        if (stageManager != null)
        {
            stageManager.StageCleared -= EnableExit;
        }
    }

    private void EnableExit()
    {
        exitCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() == null)
        {
            return;
        }

        stageManager.TryMoveToNextStage();
    }
}

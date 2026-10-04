using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialBase stepData;

    private void OnTriggerEnter2D(Collider2D collision) => TryNotifyManager(collision);
    private void OnTriggerStay2D(Collider2D collision) => TryNotifyManager(collision);

    private void TryNotifyManager(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (TutorialManager.Instance.TryStartTutorial(stepData))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
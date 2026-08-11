using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //PlayerHealth player = collision.GetComponent<PlayerHealth>();

            EventManager.TriggerEvent("OnPlayerHit");
            Debug.Log("Player Hit");
        }
    }

    public void AnimEvent_DisableEffect()
    {
        gameObject.SetActive(false);
        enemy?.outlineController?.SetDefaultOutline();
        enemy?.EnableCancel();
    }
}

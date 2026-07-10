using UnityEngine;

public class GrapEffect : MonoBehaviour
{
    [Header("연결 지점 설정")]
    public Transform startPoint;
    private Transform targetPoint;

    [Header("사슬 비주얼 요소")]
    public SpriteRenderer chainRenderer;
    public Transform hookHead;

    private void Update()
    {
        if (targetPoint == null || !targetPoint.gameObject.activeInHierarchy)
        {
            chainRenderer.enabled = false;
            hookHead.gameObject.SetActive(false);
            return;
        }

        chainRenderer.enabled = true;
        hookHead.gameObject.SetActive(true);

        Vector2 startPos = startPoint.position;
        Vector2 endPos = targetPoint.position;

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        chainRenderer.transform.position = startPos;

        float adjustedDistance = distance / Mathf.Abs(chainRenderer.transform.lossyScale.x);

        float lengthOffset = 0f;

        float exactHeight = chainRenderer.sprite.bounds.size.y;

        chainRenderer.size = new Vector2(adjustedDistance - lengthOffset, exactHeight);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        chainRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);

        hookHead.position = endPos;
        hookHead.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void StartGrabVisual(Transform target)
    {
        targetPoint = target;
    }

    public void StopGrabVisual()
    {
        targetPoint = null;
    }
}

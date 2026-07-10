using UnityEngine;

public class GrapEffect : MonoBehaviour
{
    [Header("연결 지점 설정")]
    public Transform startPoint;
    public Transform targetPoint;

    [Header("무기 비주얼 요소")]
    public SpriteRenderer chainRenderer;
    public Transform weapon;

    [Header("사슬 크기 보정")]
    public float chainHeight = 0.5f;

    private void OnEnable()
    {
        StartGrabVisual(targetPoint);
    }

    private void Update()
    {
        if (targetPoint == null || !targetPoint.gameObject.activeInHierarchy)
        {
            chainRenderer.enabled = false;
            weapon.gameObject.SetActive(false);
            return;
        }

        chainRenderer.enabled = true;
        weapon.gameObject.SetActive(true);

        Vector2 startPos = startPoint.position;
        Vector2 endPos = targetPoint.position;

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        chainRenderer.transform.position = startPos;

        chainRenderer.size = new Vector2(distance, chainHeight);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        chainRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);

        weapon.position = endPos;
        weapon.rotation = Quaternion.Euler(0, 0, angle);
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

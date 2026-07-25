using UnityEngine;
using System.Collections;

public class GrapEffect : MonoBehaviour
{
    [Header("연결 지점 설정")]
    public Transform startPoint;
    private Transform targetPoint;
    public Transform EndPoint;

    [Header("사슬 비주얼 요소")]
    public SpriteRenderer chainRenderer;
    public Transform Weapon;
    public Transform WeaponFull;

    public float WeaponSpeed;
    private bool isFailed = false;

    private void OnEnable()
    {
        chainRenderer.enabled = false;
        Weapon.gameObject.SetActive(false);
        WeaponFull.gameObject.SetActive(false);
        targetPoint = null;
        isFailed = false;
    }

    private void Update()
    {
        if (isFailed) return;

        if (targetPoint == null || !targetPoint.gameObject.activeInHierarchy)
        {
            chainRenderer.enabled = false;
            Weapon.gameObject.SetActive(false);
            WeaponFull.gameObject.SetActive(false);
            return;
        }
        grapHitVisual();
    }

    private void grapHitVisual()
    {
        chainRenderer.enabled = true;
        Weapon.gameObject.SetActive(true);

        Vector2 startPos = startPoint.position;
        Vector2 endPos = targetPoint.position;
        endPos.x += (endPos.x - startPos.x) > 0 ? -0.5f : 0.5f;

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        if (distance < 1f) StopGrabVisual();

        chainRenderer.transform.position = startPos;
        float adjustedDistance = distance / Mathf.Abs(chainRenderer.transform.lossyScale.x);
        chainRenderer.size = new Vector2(adjustedDistance, chainRenderer.sprite.bounds.size.y);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        chainRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);

        Weapon.position = endPos;
        Weapon.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void StartGrabVisual(Transform target)
    {
        targetPoint = target;
    }

    public void StopGrabVisual()
    {
        targetPoint = null;
        isFailed = false;
    }

    public void StartGrabFailVisual()
    {
        StopAllCoroutines();
        isFailed = true;

        targetPoint = null;
        Weapon.gameObject.SetActive(false);

        StartCoroutine(RetractWeaponRoutine(startPoint.position, EndPoint.position, false));
    }

    public void StartGrabFailVisualRetract()
    {
        StopAllCoroutines();

        StartCoroutine(RetractWeaponRoutine(EndPoint.position, startPoint.position, true));
    }

    private IEnumerator RetractWeaponRoutine(Vector2 startPos, Vector2 endPos, bool isReturning)
    {
        WeaponFull.gameObject.SetActive(true);
        chainRenderer.enabled = true;

        Vector2 currentWeaponPos = startPos;

        while (Vector2.Distance(currentWeaponPos, endPos) > 1f)
        {
            if (isReturning)
            {
                endPos = startPoint.position;
            }

            currentWeaponPos = Vector2.MoveTowards(currentWeaponPos, endPos, WeaponSpeed * Time.deltaTime);

            Vector2 chainStart = startPoint.position;
            Vector2 direction = currentWeaponPos - chainStart;
            float distance = direction.magnitude;

            chainRenderer.transform.position = chainStart;
            float adjustedDistance = distance / Mathf.Abs(chainRenderer.transform.lossyScale.x);
            chainRenderer.size = new Vector2(adjustedDistance, chainRenderer.sprite.bounds.size.y);

            if (distance > 0.01f)
            {
                Vector2 normDir = direction.normalized;
                chainRenderer.transform.right = normDir;
                WeaponFull.right = normDir;
            }

            WeaponFull.position = currentWeaponPos;
            yield return null;
        }

        if (isReturning)
        {
            chainRenderer.enabled = false;
            WeaponFull.gameObject.SetActive(false);
            isFailed = false;
        }
    }
}
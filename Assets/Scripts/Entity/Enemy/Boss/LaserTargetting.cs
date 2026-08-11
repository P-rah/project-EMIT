using System.Collections;
using UnityEngine;

public class LaserTargetting : MonoBehaviour
{
    private Transform playerTransform;

    [Header("References")]
    public LineRenderer laserPointer;
    public Transform firePoint;

    [Header("Attack Settings")]
    public float targetingTime = 1.5f;
    public float lockDelay = 0.2f;
    public float laserLength = 20f;
    public string attackPoolTag = "BossLaser";

    private Coroutine targetingCoroutine;

    private void OnEnable()
    {
        playerTransform = Player.InstanceTransform;
    }

    private void Awake()
    {
        if (laserPointer != null)
        {
            laserPointer.enabled = false;
        }
    }

    public void StartTargetingAttack()
    {
        if (targetingCoroutine != null)
        {
            StopCoroutine(targetingCoroutine);
        }
        targetingCoroutine = StartCoroutine(TargetingRoutine());
    }

    private IEnumerator TargetingRoutine()
    {
        if (playerTransform == null || firePoint == null || laserPointer == null) yield break;

        laserPointer.enabled = true;
        float elapsedTime = 0f;
        Vector2 targetDirection = Vector2.zero;

        while (elapsedTime < targetingTime)
        {
            elapsedTime += Time.deltaTime;

            targetDirection = ((Vector2)playerTransform.position - (Vector2)firePoint.position + new Vector2(0f,0.5f)).normalized;

            laserPointer.SetPosition(0, firePoint.position);
            laserPointer.SetPosition(1, (Vector2)firePoint.position + targetDirection * laserLength);

            yield return null;
        }

        yield return new WaitForSeconds(lockDelay);

        laserPointer.enabled = false;

        FireAttack(targetDirection);
    }

    private void FireAttack(Vector2 direction)
    {
        float spawnOffset = 0.5f;

        Vector2 spawnPos = (Vector2)firePoint.position + (direction.normalized * spawnOffset);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        ObjectPoolManager.Instance?.SpawnFromPool(attackPoolTag, spawnPos, rotation);
    }

    //public void CancelTargeting()
    //{
    //    if (targetingCoroutine != null)
    //    {
    //        StopCoroutine(targetingCoroutine);
    //        targetingCoroutine = null;
    //    }
    //    if (laserPointer != null)
    //    {
    //        laserPointer.enabled = false;
    //    }
    //}
}

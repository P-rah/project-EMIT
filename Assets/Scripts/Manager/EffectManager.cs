using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour  
{
    public static EffectManager Instance { get; private set; }

    [Header("적 피격 이펙트")]
    public GameObject enemyHitEffectPrefab;
    public int poolSize = 10;

    private Queue<GameObject> effectPool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject effect = Instantiate(enemyHitEffectPrefab, transform);
            effect.SetActive(false);
            effectPool.Enqueue(effect);
        }
    }

    public void PlayHitEffect(Vector2 position, Vector2 direction)
    {
        GameObject effect;

        if (effectPool.Count > 0)
        {
            effect = effectPool.Dequeue();
        }
        else
        {
            effect = Instantiate(enemyHitEffectPrefab, transform);
        }

        effect.transform.position = position;

        Vector3 currentScale = effect.transform.localScale;

        if (direction.x < 0)
        {
            currentScale.x = -Mathf.Abs(currentScale.x);
        }
        else
        {
            currentScale.x = Mathf.Abs(currentScale.x);
        }

        effect.transform.rotation = Quaternion.identity;
        effect.transform.localScale = currentScale;

        effect.SetActive(true);
    }

    public void ReturnEffect(GameObject effect)
    {
        effect.SetActive(false);
        effectPool.Enqueue(effect);

    }
}
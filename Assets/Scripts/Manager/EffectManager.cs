using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum DeathEffectType
{
    R1,
    R2
}

public class EffectManager : MonoBehaviour  
{
    public static EffectManager Instance { get; private set; }

    [Header("적 피격 이펙트")]
    public GameObject enemyHitEffectPrefab;
    [SerializeField]private int effectPoolSize = 10;

    [System.Serializable]
    private struct PoolItem
    {
        public DeathEffectType type;
        public GameObject prefab;
        public int initialSize;
    }
    [Header("적 사망 이펙트")]
    [SerializeField] private List<PoolItem> poolItems;

    [Header("역행 고스트")]
    public EmitTrackingEffect emitGhostPrefab;
    [SerializeField] private int emitGhostPoolSize = 100;

    private Queue<GameObject> effectPool = new Queue<GameObject>();
    private Dictionary<DeathEffectType, Queue<GameObject>> deathEffectPool;
    private Dictionary<DeathEffectType, GameObject> prefabDictionary;
    private Queue<EmitTrackingEffect> emitGhostPool = new Queue<EmitTrackingEffect>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeEffectPool();
        InitializeDeathPool();
        InitializeEmitGhostPool();
    }

    private void InitializeEffectPool()
    {
        for (int i = 0; i < effectPoolSize; i++)
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

    //사망 파편 효과
    private void InitializeDeathPool()
    {
        deathEffectPool = new Dictionary<DeathEffectType, Queue<GameObject>>();
        prefabDictionary = new Dictionary<DeathEffectType, GameObject>();

        foreach (PoolItem item in poolItems)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            prefabDictionary.Add(item.type, item.prefab);

            for (int i = 0; i < item.initialSize; i++)
            {
                GameObject obj = Instantiate(item.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            deathEffectPool.Add(item.type, objectQueue);
        }
    }

    public void PlayDeathEffect(DeathEffectType type, Vector2 position, bool isFacingLeft)
    {
        if (!deathEffectPool.ContainsKey(type))
        {
            Debug.LogError($"{type} 타입이 풀 딕셔너리에 없습니다! 인스펙터를 확인하세요.");
            return;
        }

        Queue<GameObject> queue = deathEffectPool[type];

        if (queue.Count == 0)
        {
            GameObject prefab = prefabDictionary[type];

            GameObject newObj = Instantiate(prefab, transform);
            newObj.SetActive(false);

            queue.Enqueue(newObj);
        }
        GameObject obj = queue.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(0f, isFacingLeft ? 180f : 0f, 0f);
        obj.SetActive(true);
    }

    public void ReturnDeathEffect(DeathEffectType type, GameObject effect)
    {
        effect.SetActive(false);
        deathEffectPool[type].Enqueue(effect);
    }

    //역행 고스트 이펙트
    private void InitializeEmitGhostPool()
    {
        for (int i = 0; i < emitGhostPoolSize; i++)
        {
            EmitTrackingEffect emitGhost = Instantiate(emitGhostPrefab, transform);
            emitGhost.gameObject.SetActive(false);
            emitGhostPool.Enqueue(emitGhost);
        }
    }

    public EmitTrackingEffect EnableEmitGhost()
    {
        EmitTrackingEffect emitGhost;
        if (emitGhostPool.Count > 0) emitGhost = emitGhostPool.Dequeue();
        else emitGhost = Instantiate(emitGhostPrefab, transform);

        emitGhost.gameObject.SetActive(true);
        return emitGhost;

    }

    public void ReturnEmitGhost(EmitTrackingEffect emitGhost)
    {
        emitGhost.gameObject.SetActive(false);
        emitGhostPool.Enqueue(emitGhost);
    }
}
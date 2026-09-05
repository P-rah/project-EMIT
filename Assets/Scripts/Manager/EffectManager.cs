using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour  
{
    public static EffectManager Instance { get; private set; }

    [Header("적 피격 이펙트")]
    public GameObject enemyHitEffectPrefab;
    [SerializeField]private int effectPoolSize = 10;

    [Header("사망 이펙트")]
    public GameObject deathEffectPrefab;
    [SerializeField] private int deathPoolSize = 5;

    [Header("역행 고스트")]
    public EmitTrackingEffect emitGhostPrefab;
    [SerializeField] private int emitGhostPoolSize = 100;

    private Queue<GameObject> effectPool = new Queue<GameObject>();
    private Queue<GameObject> deathEffectPool = new Queue<GameObject>();
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
        for (int i = 0; i < deathPoolSize; i++)
        {
            GameObject DieEffect = Instantiate(deathEffectPrefab, transform);
            DieEffect.SetActive(false);
            deathEffectPool.Enqueue(DieEffect);
        }
    }

    public void PlayDeathEffect(Vector2 position, bool isFacingLeft)
    {
        GameObject DieEffect;
        if (deathEffectPool.Count > 0) DieEffect = deathEffectPool.Dequeue();
        else DieEffect = Instantiate(deathEffectPrefab, transform);

        DieEffect.transform.position = position;
        DieEffect.transform.rotation = Quaternion.Euler(0f, isFacingLeft ? 180f : 0f, 0f);
        DieEffect.SetActive(true);
    }

    public void ReturnDeathEffect(GameObject effect)
    {
        effect.SetActive(false);
        deathEffectPool.Enqueue(effect);
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
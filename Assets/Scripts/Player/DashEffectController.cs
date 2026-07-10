using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DashEffectController : MonoBehaviour
{
    [Header("잔상 설정")]
    public GameObject ghostPrefab;
    public int poolSize = 10;
    public Color ghostColor = new Color(1f, 1f, 1f, 0.5f);
    public float fadeSpeed = 3f;

    [Header("생성 간격")]
    public float distanceBetweenGhosts = 1f;

    private Queue<GameObject> ghostPool;
    private bool isDashing = false;
    private Vector2 lastSpawnPosition;

    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        ghostPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ghost = Instantiate(ghostPrefab);
            ghost.SetActive(false);
            ghostPool.Enqueue(ghost);
        }
    }

    private GameObject GetGhost()
    {
        GameObject ghost = ghostPool.Dequeue();
        ghost.SetActive(true);
        ghostPool.Enqueue(ghost);
        return ghost;
    }

    public void StartDashTrail()
    {
        isDashing = true;

        // 대쉬 시작 시 현재 위치를 저장하고 첫 잔상을 바로 하나 찍습니다.
        lastSpawnPosition = transform.position;
        SpawnSingleGhost();
    }

    public void StopDashTrail()
    {
        isDashing = false;
    }

    void Update()
    {
        if (isDashing)
        {
            float currentDistance = Vector2.Distance(transform.position, lastSpawnPosition);

            if (currentDistance >= distanceBetweenGhosts)
            {
                SpawnSingleGhost();
                lastSpawnPosition = transform.position;
            }
        }
    }

    // 잔상 하나를 세팅하고 출력하는 함수
    private void SpawnSingleGhost()
    {
        GameObject ghost = GetGhost();
        DashEffectFade ghostScript = ghost.GetComponent<DashEffectFade>();

        if (ghostScript != null)
        {
            ghostScript.Setup(transform, ghostColor, fadeSpeed);
        }
    }
}

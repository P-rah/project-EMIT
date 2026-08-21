using System.Collections.Generic;
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

    // 💡 하이어라키 창을 정리할 빈 폴더 변수
    private GameObject ghostContainer;

    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        // 1. 기존 풀이 있으면 깨끗하게 비웁니다.
        if (ghostPool == null)
            ghostPool = new Queue<GameObject>();
        else
            ghostPool.Clear();

        // 2. 씬이 다시 로드될 때 남아있던 예전 폴더(찌꺼기)가 있다면 찾아서 지워줍니다.
        GameObject existingContainer = GameObject.Find("DashGhosts_Pool");
        if (existingContainer != null)
        {
            Destroy(existingContainer);
        }

        // 3. 새 폴더를 만듭니다.
        ghostContainer = new GameObject("DashGhosts_Pool");

        for (int i = 0; i < poolSize; i++)
        {
            GameObject ghost = Instantiate(ghostPrefab);

            // 💡 생성된 잔상을 폴더(Container)의 자식으로 쏙 집어넣습니다!
            ghost.transform.SetParent(ghostContainer.transform);

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

    private void OnDisable()
    {
        isDashing = false;
        if (ghostPool == null) return;

        foreach (GameObject ghost in ghostPool)
        {
            if (ghost != null)
            {
                ghost.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (ghostContainer != null)
        {
            Destroy(ghostContainer);
        }
    }
}
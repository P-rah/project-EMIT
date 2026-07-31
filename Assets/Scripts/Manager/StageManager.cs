using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("스테이지 정보")]
    public int currentEnemyCount = 0;
    public string nextSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void AddEnemy()
    {
        currentEnemyCount++;
    }

    public void EnemyDied()
    {
        currentEnemyCount--;

        if (currentEnemyCount <= 0)
        {
            StartCoroutine(StageClearRoutine());
        }
    }

    public void ReloadCurrentScene()
    {
        currentEnemyCount = 0;

        TimeManager.Instance.InstantNormalTime();
        SoundManager.instance.StopEmitBGM();
        EventManager.TriggerEvent("OnEmitAborted");
        EmitManager.Instance.EmitReset();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    private IEnumerator StageClearRoutine()
    {
        Debug.Log("스테이지 클리어");

        yield return new WaitForSeconds(2.0f);

        currentEnemyCount = 0;
        EmitManager.Instance.EmitReset();

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex + 1);
    }
}

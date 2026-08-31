using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageManager : MonoBehaviour
{
    public enum SceneIndex
    {
        TitleScene, 
        Ch1_1, 
        Ch1_2, 
        Ch1_3, 
        Ch1_4, 
        Ch1_Boss
    }

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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

    public void RestartStage()
    {
        currentEnemyCount = 0;

        TimeManager.Instance.InstantNormalTime();
        // AudioManager.Instance.StopEmitBGM();
        SoundManager.Instance.StopEmitBGM();
        EventManager.TriggerEvent("OnEmitAborted");
        EmitManager.Instance.EmitReset();

        // Scene currentScene = SceneManager.GetActiveScene();
        // SceneManager.LoadScene(currentScene.buildIndex);
        SceneControlManager.Instance.ReloadStage();
    }

    public void EndStage()
    {
        
    }

    private IEnumerator StageClearRoutine()
    {
        Debug.Log("스테이지 클리어");

        yield return new WaitForSeconds(2.0f);

        currentEnemyCount = 0;
        EmitManager.Instance.EmitReset();

        // Scene currentScene = SceneManager.GetActiveScene();
        // SceneManager.LoadScene(currentScene.buildIndex + 1);
        SceneControlManager.Instance.LoadNextStage();
    }
}

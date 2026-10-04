using System;
using UnityEngine;

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

    private bool canMoveToNextStage = false;
    private bool isTransitioning = false;

    public bool CanMoveToNextStage => canMoveToNextStage;
    public event Action StageCleared;

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

        if (currentEnemyCount <= 0 && !canMoveToNextStage)
        {
            canMoveToNextStage = true;
            StageCleared?.Invoke();
        }
    }

    public void RestartStage()
    {
        currentEnemyCount = 0;
        canMoveToNextStage = false;
        isTransitioning = false;

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

    public bool TryMoveToNextStage()
    {
        if (!canMoveToNextStage || isTransitioning)
        {
            return false;
        }

        isTransitioning = true;
        StageClearRoutine();
        return true;
    }

    private void StageClearRoutine()
    {
        Debug.Log("스테이지 클리어");

        currentEnemyCount = 0;
        EmitManager.Instance.EmitReset();

        // Scene currentScene = SceneManager.GetActiveScene();
        // SceneManager.LoadScene(currentScene.buildIndex + 1);
        SceneControlManager.Instance.LoadNextStage();
    }
}

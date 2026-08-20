using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환시 씬·에셋 로드/언로드를 자동으로 수행하기 위한 매니저 클래스
/// </summary>
public class SceneControlManager : MonoBehaviour
{
    #region Singleton
    public static SceneControlManager Instance { get; private set; }

    private void Awake()
    {
        if ( Instance != null && Instance != this )
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion 
    
    // protection level 추후에 수정 필요!
    public LoadingScreen LoadingScreen;

    private Dictionary<string, string> loadedSceneBySlot = new();
    private bool isBusy = false;

    public SceneTransitionPlan NewTransition()
    {
        return new SceneTransitionPlan();
    }

    /// <summary>
    /// 현재 씬 전환 중이 아니라면, 씬을 전환한다.
    /// </summary>
    private Coroutine ExecutePlan(SceneTransitionPlan plan)
    {
        if (isBusy)
        {
            Debug.LogWarning("Scene change is already in progress");
            return null;
        }

        isBusy = true;
        return StartCoroutine(ChangeSceneRoutine(plan));
    }

    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        if (plan.Overlay)
        {
            LoadingScreen.Show();
        }
        // 언로드할 씬들에 대해 각각 언로드를 수행
        foreach (var slotKey in plan.ScenesToUnload)
        {
            yield return UnloadSceneRoutine(slotKey);
        }
        // 참조가 끊긴 에셋을 메모리에서 정리
        if (plan.CleanupUnusedAssets) 
        {
            yield return CleanupUnusedAssetsRoutine();
        }
        // 씬 슬롯에 이미 올려진 씬을 언로드, (참고) kvp: key-value pair
        foreach(var kvp in plan.ScenesToLoad)
        {
            if (loadedSceneBySlot.ContainsKey(kvp.Key))
            {
                yield return UnloadSceneRoutine(kvp.Key);
            }
            // 씬 로드 (액티브 씬이 로드된 슬롯만 액티브 씬으로 설정)
            yield return LoadAdditiveRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
        }
        if (plan.Overlay)
        {
            LoadingScreen.Hide();
        }
        isBusy = false;
    }

    /// <summary>
    /// 추가로 씬을 로드한다.
    /// </summary>
    private IEnumerator LoadAdditiveRoutine(string slotKey, string sceneName, bool setActive)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);   
        loadOp.allowSceneActivation = false;
        while (loadOp.progress < 0.9f)
        {
            yield return null;
        }
        loadOp.allowSceneActivation = true;
        while (!loadOp.isDone)
        {
            yield return null;
        }
        if (setActive)
        {
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
        }
        loadedSceneBySlot[slotKey] = sceneName;
    }

    /// <summary>
    /// 씬을 언로드한다.
    /// </summary>
    private IEnumerator UnloadSceneRoutine(string slotKey)
    {
        // 로드된 씬이 아니라면 리턴
        if(!loadedSceneBySlot.TryGetValue(slotKey, out string sceneName)) yield break;
        // 씬 이름이 비어있다면 리턴
        if (string.IsNullOrEmpty(sceneName)) yield break;
        
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneName);
        
        if (unloadOperation != null)
        {
            while (!unloadOperation.isDone)
            {
                yield return null;
            }
        }
        
        loadedSceneBySlot.Remove(slotKey);
    }

    /// <summary>
    /// 참조 관계를 검사해 현재 어떤 오브젝트에서도 사용하지 않는 에셋을 메모리에서 찾아 해제한다.
    /// </summary>
    private IEnumerator CleanupUnusedAssetsRoutine()
    {
        AsyncOperation cleanupOperation = Resources.UnloadUnusedAssets();
        while (!cleanupOperation.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// 씬 전환 명령들을 모아두는 빌더 클래스
    /// </summary>
    public class SceneTransitionPlan
    {
        public Dictionary<string, string> ScenesToLoad { get; } = new();
        public List<string> ScenesToUnload { get; } = new();
        public string ActiveSceneName { get; private set; }
        public bool CleanupUnusedAssets { get; private set; } = false;
        public bool Overlay { get; private set; } = false;
        
        /// <summary>
        /// 로드할 씬을 추가한다. 
        /// SetActive = true로 특정 씬을 Active Scene으로 만들 수 있다.
        /// </summary>
        public SceneTransitionPlan Load(string slotKey, string sceneName, bool SetActive = false)
        {
            ScenesToLoad[slotKey] = sceneName;
            if (SetActive) ActiveSceneName = sceneName;
            return this; 
        }

        /// <summary>
        /// 언로드할 씬을 추가한다.
        /// </summary>
        public SceneTransitionPlan Unload(string slotKey)
        {
            ScenesToUnload.Add(slotKey);
            return this;
        }

        public SceneTransitionPlan WithCleanupUnusedAssets()
        {
            CleanupUnusedAssets = true;
            return this;
        }

        public SceneTransitionPlan WithOverlay()
        {
            Overlay = true;
            return this;
        }

        public Coroutine Build()
        {
            return SceneControlManager.Instance.ExecutePlan(this);
        }
    }

    public void LoadTitle()
    {
        NewTransition()
        .Load(SceneDatabase.Slots.Title, SceneDatabase.Scenes.MainMenu, true)
        .WithOverlay()
        .Build();
    }

    public void LoadFirstStage()
    {
        NewTransition()
        .Unload(SceneDatabase.Slots.Title)
        .Load(SceneDatabase.Slots.Stage, SceneDatabase.Scenes.Ch1_1, true)
        .WithOverlay()
        .WithCleanupUnusedAssets()
        .Build();
    }

    private void LoadStage(LoadIndex loadIndex)
    {
        string currentStage = loadedSceneBySlot[SceneDatabase.Slots.Stage];
        int nextSceneBuildIndex = SceneManager.GetSceneByName(currentStage).buildIndex + (int) loadIndex;
        string nextStage = SceneManager.GetSceneByBuildIndex(nextSceneBuildIndex).name;

        NewTransition()
        .Load(SceneDatabase.Slots.Stage, nextStage, true)
        .WithOverlay()
        .WithCleanupUnusedAssets()
        .Build();
    }

    public void LoadPrevStage()
    {
        LoadStage(LoadIndex.Previous);
    }

    public void LoadNextStage()
    {
        LoadStage(LoadIndex.Next);
    }

    public void ReloadStage()
    {
        LoadStage(LoadIndex.Current);
    }
    public enum LoadIndex
    {
        Previous = -1,
        Current = 0,
        Next = 1
    }
}

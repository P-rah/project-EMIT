using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
/// <summary>
/// 특정 유형의 씬이 로드되기 이전에 실행하는 Base Class
/// 특정 씬 유형의 Single Entry Point
/// </summary>
public class SceneInitiatorBase : MonoBehaviour
{
    [SerializeField] protected AudioClip bgm;

    private void Start()
    {
        Initiate();
    }

    protected virtual async void Initiate()
    {
        BindObjects();
        // _loadingScreen.Show();
        await InitializeObjects();
        await CreateObjects();
        // _loadingScreen.Hide();
        PrepareGame();
    }

    /// <summary> 오브젝트 Instantiate만 담당 </summary>
    protected virtual void BindObjects()
    {
        // someManager = Instantiate(someManager);
    }

    // <summary> 씬 내 필요한 오브젝트들끼리 바인딩 </summary>
    protected virtual async UniTask InitializeObjects()
    {
        // _sceneControlMananger.LoadingScreen = _loadingScreen;
    }

    // <summary> 동적으로 생성될 오브젝트 생성 </summary>
    protected virtual async UniTask CreateObjects()
    {
        
    }

    public virtual void PrepareGame()
    {
        
    }
}

using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Threading;

public class GameInitiator : MonoBehaviour
{
    /*
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private Transform _particleArea;
    [SerializeField] private EventSystem _eventSystem;
    */

    [SerializeField] private LoadingScreen _loadingScreen;

    [Header("Managers")]
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private SceneControlManager _sceneControlMananger;

    private async void Start()
    {
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        BindObjects();
        // _loadingScreen.Show();
        await InitializeObjects();
        await CreateObjects();
        await DoSomething();
        // _loadingScreen.Hide();
        PrepareGame();
    }

    private void BindObjects()
    {
        /*
        _mainCamera = Instantiate(_mainCamera);
        _canvas = Instantiate(_canvas);
        _particleSystem = Instantiate(_particleSystem);
        _particleArea = Instantiate(_particleArea);
        _eventSystem = Instantiate(_eventSystem);
        */
        
        _loadingScreen = Instantiate(_loadingScreen);
        
        _gameManager = Instantiate(_gameManager);
        _sceneControlMananger = Instantiate(_sceneControlMananger);
    }

    private async UniTask InitializeObjects()
    {
        /*
        _canvas.worldCamera = _mainCamera;
        _particleSystem.trigger.AddCollider(_particleArea);
        _titleMenuController = _canvas.GetComponent<TitleMenuController>();
        _titleMenuController.NewGameButton.onClick.AddListener(_sceneControlMananger.LoadFirstStage);
        _titleMenuController.QuitButton.onClick.AddListener(_gameManager.QuitGame);
        */
        _sceneControlMananger.LoadingScreen = _loadingScreen;
    }

    private async UniTask CreateObjects()
    {
            
    }

    private async UniTask DoSomething()
    {
        await UniTask.Delay(1000);
    }

    private void PrepareGame()
    {
        SceneControlManager.Instance.LoadTitle();
    }
}

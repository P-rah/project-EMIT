using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Threading;

public class GameInitiator : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LoadingScreen _loadingScreen;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private Transform _particleArea;
    [SerializeField] private EventSystem _eventSystem;

    private async void Start()
    {
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        BindObjects();
        _loadingScreen.Show();
        await InitializeObjects();
        await CreateObjects();
        await DoSomething();
        _loadingScreen.Hide();
        PrepareGame();
    }

    private void BindObjects()
    {
        _mainCamera = Instantiate(_mainCamera);
        _loadingScreen = Instantiate(_loadingScreen);
        _canvas = Instantiate(_canvas);
        _particleSystem = Instantiate(_particleSystem);
        _particleArea = Instantiate(_particleArea);
        _eventSystem = Instantiate(_eventSystem);
    }

    private async UniTask InitializeObjects()
    {
        _canvas.worldCamera = _mainCamera;
        _particleSystem.trigger.AddCollider(_particleArea);
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
        
    }
}

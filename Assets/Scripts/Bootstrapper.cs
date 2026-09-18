using UnityEngine;

public static class Bootstrapper
{
    // 씬이 로드되기 전에 명시적으로 게임매니저나 코어 시스템 스크립트들을 생성한다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Systems")));
        Debug.Log("Core Systems loaded");
    }
}
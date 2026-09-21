using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * 게임 이벤트 제어용 매니저 클래스
 */

[DefaultExecutionOrder(-1000)]
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    // 이벤트 보관 딕셔너리
    private Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

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

    // 리스너 등록
    public static void StartListening(string eventName, Action listener)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            Instance.eventDictionary[eventName] = Delegate.Combine(thisEvent, listener);
        else
            Instance.eventDictionary.Add(eventName, listener);
    }

    // 리스너 해제
    public static void StopListening(string eventName, Action listener)
    {
        if (Instance == null) return;
        if (Instance.eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            Instance.eventDictionary[eventName] = Delegate.Remove(thisEvent, listener);
    }

    // 이벤트 발생
    public static void TriggerEvent(string eventName)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            (thisEvent as Action)?.Invoke();
    }

    // 매개변수<T> 에 대한 오버로드
    public static void StartListening<T>(string eventName, Action<T> listener)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            Instance.eventDictionary[eventName] = Delegate.Combine(thisEvent, listener);
        else
            Instance.eventDictionary.Add(eventName, listener);
    }

    public static void StopListening<T>(string eventName, Action<T> listener)
    {
        if (Instance == null) return;
        if (Instance.eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
            Instance.eventDictionary[eventName] = Delegate.Remove(thisEvent, listener);
    }

    public static void TriggerEvent<T>(string eventName, T parameter)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out Delegate thisEvent))
        {
            if (thisEvent is Action<T> action)
            {
                action.Invoke(parameter);
            }
            else
            {
                Debug.LogWarning($"이벤트 '{eventName}'의 매개변수 타입이 일치하지 않습니다!");
            }
        }
    }
}

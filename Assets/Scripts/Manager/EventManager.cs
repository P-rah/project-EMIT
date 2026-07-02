using System;
using System.Collections.Generic;
using UnityEngine;
/*
 * 게임 이벤트 제어용 매니저 클래스
 */
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    // 이벤트 보관 딕셔너리
    private Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

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
        if (Instance.eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent += listener;
            Instance.eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    // 리스너 해제
    public static void StopListening(string eventName, Action listener)
    {
        if (Instance == null) return;

        if (Instance.eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent -= listener;
            Instance.eventDictionary[eventName] = thisEvent;
        }
    }

    // 이벤트 발생
    public static void TriggerEvent(string eventName)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent?.Invoke();
        }
    }
}

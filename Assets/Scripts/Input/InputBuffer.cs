using System;
using System.Collections.Generic;
using UnityEngine;

public enum Command
{
    Jump,
    Dash
}

public enum InputBufferEvent
{
    Registered,
    Refreshed,
    Consumed,
    Expired,
    Cleared
}

public class InputBuffer : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float lifetimeSeconds = 0.12f;

    private readonly Dictionary<Command, PendingInput> pending = new();
    private readonly List<Command> expired = new();
    private TimeManager timeManager;

    public event Action<Command, InputBufferEvent> Changed;
    public int ConsumedCount { get; private set; }
    public int ExpiredCount { get; private set; }
    public float LifetimeSeconds
    {
        get => lifetimeSeconds;
        set
        {
            Clear();
            lifetimeSeconds = Mathf.Max(0.01f, value);
        }
    }

    // FixedUpdate 안에서도 같은 실제 시각을 사용한다. 물리 시각으로 바뀌는 unscaledTime과 구분한다.
    private static double Now => Time.realtimeSinceStartupAsDouble;
    private bool IsPaused => Time.timeScale <= 0f ||
        (TimeManager.Instance != null && TimeManager.Instance.IsPaused);

    private readonly struct PendingInput
    {
        public readonly IBufferedCommand Command;
        public readonly double ExpiresAt;

        public PendingInput(IBufferedCommand command, double expiresAt)
        {
            Command = command;
            ExpiresAt = expiresAt;
        }
    }

    private void OnEnable()
    {
        timeManager = TimeManager.Instance;
        if (timeManager != null) timeManager.Paused += Clear;
    }

    private void OnDisable()
    {
        if (timeManager != null) timeManager.Paused -= Clear;
        timeManager = null;
        Clear();
    }

    private void Update()
    {
        if (IsPaused) Clear();
        else RemoveExpired();
    }

    public void Register(Command type, IBufferedCommand command)
    {
        if (!isActiveAndEnabled || IsPaused || command == null) return;

        RemoveExpired();
        bool replacing = pending.ContainsKey(type);
        pending[type] = new PendingInput(command, Now + lifetimeSeconds);
        Changed?.Invoke(type, replacing ? InputBufferEvent.Refreshed : InputBufferEvent.Registered);
    }

    // 실행은 이동 담당자의 FixedUpdate에서만 요청한다.
    public bool TryExecute(Command type)
    {
        if (!isActiveAndEnabled || IsPaused) return false;
        RemoveExpired();
        if (!pending.TryGetValue(type, out PendingInput entry)) return false;
        if (!entry.Command.TryExecute()) return false;

        pending.Remove(type);
        ConsumedCount++;
        Changed?.Invoke(type, InputBufferEvent.Consumed);
        return true;
    }

    public float GetRemainingSeconds(Command type)
    {
        return pending.TryGetValue(type, out PendingInput entry)
            ? Mathf.Max(0f, (float)(entry.ExpiresAt - Now))
            : 0f;
    }

    public void RemoveExpired()
    {
        double now = Now;
        expired.Clear();
        foreach (var entry in pending)
        {
            if (now >= entry.Value.ExpiresAt) expired.Add(entry.Key);
        }

        foreach (Command type in expired)
        {
            pending.Remove(type);
            ExpiredCount++;
            Changed?.Invoke(type, InputBufferEvent.Expired);
        }
    }

    public void Clear()
    {
        if (pending.Count == 0) return;
        Command[] cleared = new Command[pending.Count];
        pending.Keys.CopyTo(cleared, 0);
        pending.Clear();
        foreach (Command type in cleared) Changed?.Invoke(type, InputBufferEvent.Cleared);
    }
}

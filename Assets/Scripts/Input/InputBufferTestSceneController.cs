using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// InputBufferTestScene 전용 수동 테스트 UI. 실제 PlayerInput / 이동 / Animator 경로를 사용한다.
public class InputBufferTestSceneController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private TempPlayerMovement movement;
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private BoxCollider2D[] arenaSurfaces;

    private readonly Queue<string> history = new();
    private readonly float[] lifetimes = { 0.08f, 0.12f, 0.2f, 0.3f };
    private InputBuffer buffer;
    private TimeManager timeManager;
    private Sprite arenaSprite;
    private bool slowMotion;
    private bool pausedByTest;
    private bool changedTimeScale;
    private int lifetimeIndex = 1;

    private void Awake()
    {
        buffer = GetComponent<InputBuffer>();
    }

    private void OnEnable()
    {
        buffer.Changed += OnBufferChanged;
    }

    private void Start()
    {
        timeManager = TimeManager.Instance;
        CreateArenaVisuals();
        AddHistory("Ready - Jump / Dash buffer enabled");
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || timeManager == null) return;

        if (keyboard.pKey.wasPressedThisFrame) TogglePause();
        if (timeManager.IsPaused) return;

        if (keyboard.f1Key.wasPressedThisFrame)
        {
            input.SetInputBufferEnabled(!input.UsingInputBuffer);
            AddHistory(input.UsingInputBuffer ? "Mode: BUFFER" : "Mode: LEGACY");
        }

        if (keyboard.f2Key.wasPressedThisFrame)
        {
            slowMotion = !slowMotion;
            changedTimeScale = true;
            if (slowMotion) timeManager.TimeStop();
            else timeManager.InstantNormalTime();
            AddHistory(slowMotion ? "Slow motion: 0.1x" : "Normal speed: 1x");
        }

        if (keyboard.f3Key.wasPressedThisFrame)
        {
            lifetimeIndex = (lifetimeIndex + 1) % lifetimes.Length;
            buffer.LifetimeSeconds = lifetimes[lifetimeIndex];
            AddHistory($"Lifetime: {buffer.LifetimeSeconds * 1000f:0} ms");
        }

        if (keyboard.f4Key.wasPressedThisFrame && !movement.IsDashing &&
            !Player.Combat.IsAttackingSweep && !Player.Combat.IsAttackingGrab)
        {
            input.ResetAllInputs();
            movement.StopMovement();
            playerBody.position = new Vector2(0f, 3f);
            AddHistory("Drop: press Jump just before landing");
        }
    }

    private void TogglePause()
    {
        if (timeManager.IsPaused)
        {
            timeManager.Resume();
            GlobalInputHandler.Instance.SetGameplayInputEnabled(true);
            pausedByTest = false;
            AddHistory("Resumed - buffer empty");
        }
        else
        {
            timeManager.Pause();
            GlobalInputHandler.Instance.SetGameplayInputEnabled(false);
            pausedByTest = true;
            AddHistory("Paused - inputs cleared");
        }
    }

    private void CreateArenaVisuals()
    {
        Texture2D texture = Texture2D.whiteTexture;
        arenaSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), texture.width);
        arenaSprite.name = "Input Buffer Test Surface";

        foreach (BoxCollider2D surface in arenaSurfaces)
        {
            GameObject visual = new GameObject("Surface Visual");
            visual.transform.SetParent(surface.transform, false);
            visual.transform.localPosition = surface.offset;
            visual.transform.localScale = new Vector3(surface.size.x, surface.size.y, 1f);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = arenaSprite;
            renderer.color = new Color(0.25f, 0.65f, 0.7f);
        }
    }

    private void OnBufferChanged(Command command, InputBufferEvent result)
    {
        AddHistory($"{command}: {result}");
    }

    private void AddHistory(string message)
    {
        history.Enqueue($"{Time.realtimeSinceStartup:0.00}  {message}");
        while (history.Count > 5) history.Dequeue();
    }

    private void OnGUI()
    {
        if (input == null || movement == null || buffer == null) return;

        Matrix4x4 previousMatrix = GUI.matrix;
        float scale = Mathf.Min(Screen.width / 960f, Screen.height / 600f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        GUI.Box(new Rect(12f, 12f, 440f, 358f), "INPUT BUFFER TEST");
        float y = 40f;
        DrawLine(ref y, $"Mode: {(input.UsingInputBuffer ? "BUFFER" : "LEGACY")}    " +
            $"Lifetime: {buffer.LifetimeSeconds * 1000f:0} ms    Scale: {Time.timeScale:0.00}x");
        DrawLine(ref y, $"Grounded: {movement.IsGrounded}    Dash ready: {movement.CanDash}    " +
            $"Dashing: {movement.IsDashing}");
        DrawLine(ref y, $"State: {Player.State.CurrentState}    " +
            $"Paused: {(timeManager != null && timeManager.IsPaused)}");
        DrawLine(ref y, $"Jump: {buffer.GetRemainingSeconds(Command.Jump) * 1000f:0} ms    " +
            $"Dash: {buffer.GetRemainingSeconds(Command.Dash) * 1000f:0} ms");
        DrawLine(ref y, $"Consumed: {buffer.ConsumedCount}    Expired: {buffer.ExpiredCount}    " +
            "Priority: Dash > Jump");
        y += 6f;
        DrawLine(ref y, "A / D: move    Space: jump    Shift: dash");
        DrawLine(ref y, "Mouse L / R: sweep / grab (existing attack logic)");
        DrawLine(ref y, "F1: buffer / legacy    F2: 1x / 0.1x    P: pause / resume");
        DrawLine(ref y, "F3: lifetime    F4: drop from above (when not attacking)");
        y += 6f;
        foreach (string entry in history) DrawLine(ref y, entry);
        GUI.matrix = previousMatrix;
    }

    private static void DrawLine(ref float y, string text)
    {
        GUI.Label(new Rect(24f, y, 420f, 20f), text);
        y += 21f;
    }

    private void OnDisable()
    {
        if (buffer != null) buffer.Changed -= OnBufferChanged;
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            if (pausedByTest)
            {
                timeManager.Resume();
                if (GlobalInputHandler.Instance != null)
                    GlobalInputHandler.Instance.SetGameplayInputEnabled(true);
            }
            if (changedTimeScale) timeManager.InstantNormalTime();
        }
        if (arenaSprite != null) Destroy(arenaSprite);
    }
}

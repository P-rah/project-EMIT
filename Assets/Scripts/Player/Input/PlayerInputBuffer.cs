using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInputBuffer : MonoBehaviour
{
    private struct BufferedInput
    {
        public int PressedFrame { get; private set; }
        public int ExpiresAtFrame { get; private set; }

        public void Store(int pressedFrame, int validFrameCount)
        {
            if (validFrameCount <= 0)
            {
                Clear();
                return;
            }

            PressedFrame = pressedFrame;
            ExpiresAtFrame = pressedFrame + validFrameCount - 1;
        }

        public bool Consume(int currentFrame)
        {
            bool isValid = IsValid(currentFrame);
            Clear();
            return isValid;
        }

        public bool IsValid(int currentFrame)
        {
            return PressedFrame >= 0 && currentFrame <= ExpiresAtFrame;
        }

        public void Clear()
        {
            PressedFrame = -1;
            ExpiresAtFrame = -1;
        }
    }

    [SerializeField] private PlayerInputHandler inputHandler;

    [Header("입력별 버퍼 유효 프레임")]
    [Tooltip("0이면 저장하지 않고, 1이면 입력된 프레임에만 유효합니다.")]
    [Min(0)]
    [SerializeField] private int jumpExpirationTicks = 5;
    [Min(0)]
    [SerializeField] private int downExpirationTicks = 5;
    [Min(0)]
    [SerializeField] private int dashExpirationTicks = 5;
    [Min(0)]
    [SerializeField] private int sweepExpirationTicks = 5;
    [Min(0)]
    [SerializeField] private int grabExpirationTicks = 5;

    private BufferedInput jumpInput;
    private BufferedInput downInput;
    private BufferedInput dashInput;
    private BufferedInput sweepInput;
    private BufferedInput grabInput;

    public bool HasBufferedJump => jumpInput.IsValid(Time.frameCount);
    public bool HasBufferedDown => downInput.IsValid(Time.frameCount);
    public bool HasBufferedDash => dashInput.IsValid(Time.frameCount);
    public bool HasBufferedAttackSweep => sweepInput.IsValid(Time.frameCount);
    public bool HasBufferedAttackGrab => grabInput.IsValid(Time.frameCount);

    private void Awake()
    {
        if (inputHandler == null)
        {
            inputHandler = GetComponent<PlayerInputHandler>();
        }
    }

    private void OnEnable()
    {
        ClearAll();
    }

    private void Update()
    {
        CaptureInputs();
    }

    private void OnDisable()
    {
        ClearAll();
    }

    private void OnValidate()
    {
        jumpExpirationTicks = Mathf.Max(0, jumpExpirationTicks);
        downExpirationTicks = Mathf.Max(0, downExpirationTicks);
        dashExpirationTicks = Mathf.Max(0, dashExpirationTicks);
        sweepExpirationTicks = Mathf.Max(0, sweepExpirationTicks);
        grabExpirationTicks = Mathf.Max(0, grabExpirationTicks);
    }

    private void CaptureInputs()
    {
        if (inputHandler == null)
        {
            return;
        }

        int currentFrame = Time.frameCount;

        Capture(ref jumpInput, inputHandler.JumpPressedFrame, jumpExpirationTicks, currentFrame);
        Capture(ref downInput, inputHandler.DownPressedFrame, downExpirationTicks, currentFrame);
        Capture(ref dashInput, inputHandler.DashPressedFrame, dashExpirationTicks, currentFrame);
        Capture(
            ref sweepInput,
            inputHandler.AttackSweepPressedFrame,
            sweepExpirationTicks,
            currentFrame
        );
        Capture(
            ref grabInput,
            inputHandler.AttackGrabPressedFrame,
            grabExpirationTicks,
            currentFrame
        );
    }

    public bool ConsumeJump()
    {
        return jumpInput.Consume(Time.frameCount);
    }

    public bool ConsumeDown()
    {
        return downInput.Consume(Time.frameCount);
    }

    public bool ConsumeDash()
    {
        return dashInput.Consume(Time.frameCount);
    }

    public bool ConsumeAttackSweep()
    {
        return sweepInput.Consume(Time.frameCount);
    }

    public bool ConsumeAttackGrab()
    {
        return grabInput.Consume(Time.frameCount);
    }

    public void ClearAll()
    {
        jumpInput.Clear();
        downInput.Clear();
        dashInput.Clear();
        sweepInput.Clear();
        grabInput.Clear();
    }

    private static void Capture(
        ref BufferedInput input,
        int pressedFrame,
        int validFrameCount,
        int currentFrame
    )
    {
        if (pressedFrame == currentFrame)
        {
            input.Store(pressedFrame, validFrameCount);
        }
    }
}

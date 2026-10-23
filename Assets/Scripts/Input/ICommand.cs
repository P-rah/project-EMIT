using System.Threading.Tasks;
using UnityEngine;

public interface ICommand
{
    Task Execute();
}

public abstract class PlayerCommand : ICommand
{
    protected readonly Player player;

    protected PlayerCommand(Player player)
    {
        this.player = player;
    }

    public abstract Task Execute();
}

// 기존 비동기 계약을 보존하고, 버퍼에는 행동 시작의 성공 여부를 반환한다.
public interface IBufferedCommand
{
    bool TryExecute();
}

public sealed class BufferedJumpCommand : IBufferedCommand
{
    private readonly TempPlayerMovement movement;

    public BufferedJumpCommand(TempPlayerMovement movement)
    {
        this.movement = movement;
    }

    public bool TryExecute() => movement != null && movement.TryBufferedJump();
}

public sealed class BufferedDashCommand : IBufferedCommand
{
    private readonly TempPlayerMovement movement;

    public BufferedDashCommand(TempPlayerMovement movement)
    {
        this.movement = movement;
    }

    public bool TryExecute() => movement != null && movement.TryBufferedDash();
}

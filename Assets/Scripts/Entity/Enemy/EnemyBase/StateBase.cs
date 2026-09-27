[System.Serializable]
public abstract class StateBase<T> where T : IStateMachine
{
    /// <summary>
    /// 책임 및 기능: 상속 구조의 상태 관리 추상 클래스
    /// </summary>

    public T owner { get; protected set; }

    public StateBase(T owner)
    {
        this.owner = owner;
    }

    public abstract void EnterState();

    public virtual void UpdateState() { }

    public abstract void ExitState();

    public virtual void Move() { }

    public virtual bool CanBeCancelled()
    {
        return true;
    }

    public virtual void OnReceiveKnockback() { }

    public virtual void OnReceiveGrab() { }
}
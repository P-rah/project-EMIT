// 원래는 stateBase를 바탕으로 각 적마다 State를 따로 선언하려 하였으나.
// 상태 상속 구조 개편으로 더 이상 사용되지 않음


//public abstract class StateBase<T> : IState where T : IStateMachine
//{
//    /// <summary>
//    /// 책임 및 기능: 상속 구조의 상태 관리 추상 클래스
//    /// </summary>

//    public T owner { get; protected set; }

//    public StateBase(T owner)
//    {
//        this.owner = owner;
//    }

//    public abstract void EnterState();

//    public virtual void UpdateState() { }

//    public abstract void ExitState();

//    public virtual void Move() { }

//    public virtual bool CanBeCancelled()
//    {
//        return true;
//    }

//    public virtual void OnReceiveKnockback() { }

//    public virtual void OnReceiveGrab() { }
//}

using UnityEngine;

public interface IEmit
{
    /*
    Emit 가능한 오브젝트는 어떻게 키프레임을 저장해야 하는가?

    1. 오브젝트 초기화 시에는 키프레임을 모두 Initial Position으로 초기화한다.
        - Initial Position: 오브젝트 Awake() 시 생성시킬 Position
        - 씬 로딩 이후에 동적으로 Awake/Enable된다면, 해당 시점의 Position
    2. Enable 상태일 때 키프레임은 정상적으로 찍어나간다.
    3. Emit이 발동된 상태에서도 키프레임을 찍는 것이 Behavior상 안전하기는 하다.
    4. 키프레임 저장은 FIFO, 꺼내기는 LIFO이므로 dequeue를 이용한다.
    5. FixedUpdate 기반으로 우선 키프레임을 찍는다. 
    */

    public void RecordFrame();
    public void BeginEmit();
    public void EmitFrame();
    public void EndEmit();
    public void ResetFrame();
}

public interface IEmit

    /*
     * 역행 가능한 오브젝트의 인터페이스
     */
{
    public void RecordFrame();
    public void BeginEmit();
    public void EmitFrame();
    public void EndEmit();
    public void ResetFrame();
}

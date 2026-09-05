public interface IEmit

    /*
     * EmitManager 인터페이스
     */
{
    public void RecordFrame();
    public void BeginEmit();
    public void EmitFrame();
    public void EndEmit();
    public void ResetFrame();
    public void BeginGhost();
    public void EndGhost();
}

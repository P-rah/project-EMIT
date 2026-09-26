using UnityEngine;

public class StageSceneInitiator : SceneInitiatorBase
{
    [Header("EmitManager")]
    [SerializeField] EmitManager emitManager;

    [Header("StageManager")]
    [SerializeField] StageManager stageManager;

    public override void PrepareGame()
    {
        SoundManager.Instance.PlayBGM(base.bgm, true);
    }

}

using UnityEngine;

public class TitleSceneInitiator : SceneInitiatorBase
{
    public override void PrepareGame()
    {
        SoundManager.Instance.PlayBGM(base.bgm, true);
        CursorManager.Instance.setCursorVisible(true);
    }
}

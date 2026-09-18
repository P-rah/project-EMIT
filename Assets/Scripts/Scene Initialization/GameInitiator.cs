using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 게임을 시작하기 위한 코어 시스템을 로딩하는 클래스.
/// 게임 전체의 Single Entry Point
/// </summary>
public class GameInitiator : SceneInitiatorBase
{
    public override void PrepareGame()
    {
        SceneControlManager.Instance.LoadTitle();
    }
}

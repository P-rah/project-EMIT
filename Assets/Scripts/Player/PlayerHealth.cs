using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public void PlayerDead()
    {
        Player.State.isDeath = true;
        Player.Movement.stopMove();
        Debug.Log("Player Dead");
    }
    public void AnimEvent_DeadEmitActive()
    {
        EventManager.TriggerEvent("OnEmitPressed");
    }
    public void AnimEvent_SceneReset()
    {
        StageManager.Instance.ReloadCurrentScene();
    }
}

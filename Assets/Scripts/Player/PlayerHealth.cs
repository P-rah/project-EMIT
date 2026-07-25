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
        //EventManager.TriggerEvent("EmitReset");
        Destroy(EffectManager.Instance.gameObject);
        Destroy(EmitManager.Instance.gameObject);
        Destroy(EventManager.Instance.gameObject);
        TimeManager.Instance.InstantNormalTime();
        Destroy(TimeManager.Instance.gameObject);
        //ÀÏ´Ü ÀÓ½Ã·Î ÆÄ±«ÇØµÒ

        GameManager.Instance.ReloadCurrentScene();
    }
}

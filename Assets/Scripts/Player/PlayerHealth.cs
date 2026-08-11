using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int playerHealthMax = 3;
    public int currentPlayerHealth;

    private void Start()
    {
        currentPlayerHealth = playerHealthMax;
    }

    private void OnEnable()
    {

        EventManager.StartListening("OnPlayerHit", decreasePlayerHealth);
    }
    private void OnDisable()
    {
        EventManager.StopListening("OnPlayerHit", decreasePlayerHealth);
    }

    public void setPlayerHealth(int playerHealth)
    {
        if (playerHealth > playerHealthMax) return;
        this.currentPlayerHealth = playerHealth;
        Player.UIController.UpdateHealthBar();
    }

    private void decreasePlayerHealth()
    {
        this.currentPlayerHealth -= 1;
        Player.UIController.UpdateHealthBar();
        if (currentPlayerHealth <= 0)
        {
            PlayerDead();
        }
    }

    private void PlayerDead()
    {
        Player.State.IsDead = true;
        Player.Movement.StopMove();
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

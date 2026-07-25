using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public void PlayerDead()
    {
        Player.State.isDeath = true;
        Debug.Log("Player Dead");
    }
}

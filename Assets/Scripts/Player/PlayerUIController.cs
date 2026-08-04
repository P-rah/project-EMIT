using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("UI 설정")]
    public Image healthBarImage;
    public Image attackBarImage;

    public void UpdateHealthBar()
    {
        float UIRatio = (float)Player.PlayerHealth.currentPlayerHealth / Player.PlayerHealth.playerHealthMax;

        healthBarImage.fillAmount = UIRatio;
    }

    public void UpdateAttackBar()
    {
        float UIRatio = (float)Player.Combat.currentAttackCount / Player.Combat.maxAttackCount;

        attackBarImage.fillAmount = UIRatio;
    }
}

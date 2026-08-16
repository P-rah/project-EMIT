using UnityEngine;

public class PowerStackUIController : MonoBehaviour
{
    [SerializeField] private Sprite[] stackSprites;

    [SerializeField] private SpriteRenderer stackRenderer;

    private const int maxStack = 5;

    void Start()
    {
        UpdateStackDisplay(0);
    }

    public void SetStack(int stack)
    {
        if (stack >= -maxStack && stack <= maxStack)
        {
            UpdateStackDisplay(stack);
        }
        else return;
    }

    private void UpdateStackDisplay(int stack)
    {
        if (stackSprites.Length == 0 || stack == 0)
        {
            stackRenderer.sprite = null;
            return;
        }

        int spriteIndex;

        if (stack > 0)
            spriteIndex = Mathf.Clamp(stack - 1, 0, stackSprites.Length - 1);
        else
            spriteIndex = Mathf.Clamp(-stack - 1, 0, stackSprites.Length - 1);

        stackRenderer.sprite = stackSprites[spriteIndex];
    }

    public void setPowerStackFlip(bool flip)
    {
        stackRenderer.flipX = flip;
    }
}

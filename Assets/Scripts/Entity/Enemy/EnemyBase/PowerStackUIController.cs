using UnityEngine;

public class PowerStackUIController : MonoBehaviour
{
    [SerializeField] private Sprite[] stackSprites;

    [SerializeField] private SpriteRenderer stackRenderer;

    private const int maxStack = 5;
    private bool ownerFacingLeft;
    private int currentStack;

    void Start()
    {
        UpdateStackDisplay(0);
    }

    public void SetStack(int stack)
    {
        if (stack >= -maxStack && stack <= maxStack)
        {
            UpdateStackDisplay(stack);
            currentStack = stack;
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
        {
            spriteIndex = Mathf.Clamp(stack - 1, 0, stackSprites.Length - 1);
            CalculateStackDir(true);
        }
        else
        {
            spriteIndex = Mathf.Clamp(-stack - 1, 0, stackSprites.Length - 1);
            CalculateStackDir(false);
        }

        stackRenderer.sprite = stackSprites[spriteIndex];
    }

    private void CalculateStackDir(bool isStackPositive)
    {
        if(ownerFacingLeft == isStackPositive)
            stackRenderer.flipX = false;
        else
            stackRenderer.flipX = true;
    }

    public void OwnerFacingDir(bool isFacingLeft)
    {
        this.ownerFacingLeft = isFacingLeft;
        CalculateStackDir(currentStack > 0);
    }
}

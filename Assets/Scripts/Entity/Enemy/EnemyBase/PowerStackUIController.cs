using UnityEngine;

public class PowerStackUIController : MonoBehaviour
{
    [SerializeField] private PowerStackAnimator[] powerStackAnimators;
    private Transform Transform;

    private const int maxStack = 5;
    private bool ownerFacingLeft;
    private int currentStack;

    private void OnEnable()
    {
        Transform = GetComponent<Transform>();
    }

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
        //if (stack == 0)
        //{
        //    foreach (PowerStackAnimator powerStack in powerStackAnimators)
        //    {
        //        powerStack.HidePowerStack();
        //    }
        //    return;
        //}

        if (stack > 0)
        {
            for(int i = 0; i < powerStackAnimators.Length; i++)
            {
                if(i < stack) powerStackAnimators[i].ShowPowerStack();
                else powerStackAnimators[i].HidePowerStack();
            }   
            CalculateStackDir(true);
        }
        else
        {
            for (int i = 0; i < powerStackAnimators.Length; i++)
            {
                if (i < -stack) powerStackAnimators[i].ShowPowerStack();
                else powerStackAnimators[i].HidePowerStack();
            }
            CalculateStackDir(false);
        }
    }

    private void CalculateStackDir(bool isStackPositive)
    {
        if(ownerFacingLeft == isStackPositive)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    public void OwnerFacingDir(bool isFacingLeft)
    {
        this.ownerFacingLeft = isFacingLeft;
        CalculateStackDir(currentStack > 0);
    }
}

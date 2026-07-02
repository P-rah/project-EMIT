using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private PlayerInputHandler input;
    private bool isFacingRight = true;

    private void Awake()
    {
        input = GetComponentInParent<PlayerInputHandler>();
    }

    private void Update()
    {
        if (input.MoveInput.x > 0 && !isFacingRight) Flip();
        else if (input.MoveInput.x < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        this.transform.Rotate(0f, 180f, 0f);
    }
}

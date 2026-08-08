using UnityEngine;

public class OutlineController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Outline Colors")]
    public Color defaultColor = Color.white;
    public Color attackColor = Color.red;
    public Color emitColor = Color.yellow;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetDefaultOutline()
    {
        if (spriteRenderer != null)
            spriteRenderer.material.SetColor("_OutlineColor", defaultColor);
    }

    public void SetAttackOutline()
    {
        if (spriteRenderer != null)
            spriteRenderer.material.SetColor("_OutlineColor", attackColor);
    }

    public void SetEmitOutline()
    {
        if (spriteRenderer != null)
            spriteRenderer.material.SetColor("_OutlineColor", emitColor);
    }
}
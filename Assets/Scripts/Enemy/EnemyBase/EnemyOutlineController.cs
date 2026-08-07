using UnityEngine;

public class EnemyOutlineController : MonoBehaviour
{
    private Enemy _enemy;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;
    private Color attackColor;
    private Color emitColor;

    private void OnEnable()
    {
        _enemy = GetComponent<Enemy>();
        spriteRenderer = _enemy.GetComponent<SpriteRenderer>();
        defaultColor = _enemy.defaultOutlineColor;
        attackColor = _enemy.attackOutlineColor;
        emitColor = _enemy.emitOutlineColor;

    }

    public void setDefaultOutline()
    {
        spriteRenderer.material.SetColor("_OutlineColor", defaultColor);
    }

    public void setAtrackOutline()
    {
        spriteRenderer.material.SetColor("_OutlineColor", attackColor);
    }

    public void setEmitOutline()
    {
        spriteRenderer.material.SetColor("_OutlineColor", emitColor);
    }
}

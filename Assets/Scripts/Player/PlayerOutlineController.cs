using UnityEngine;

public class PlayerOutlineController : MonoBehaviour
{
    private SpriteRenderer SpriteRenderer;
    private Color playerOutlineColorOrg;
    private Color playerOutlineEmitable; 

    private void Awake()
    {
        SpriteRenderer = Player.Instance.spriteRenderer;
        playerOutlineColorOrg = SpriteRenderer.material.GetColor("_OutlineColor");
        playerOutlineEmitable = new Color(1f, 1f, 0f, 1f);
    }

    public void setOutlineColor(Color color, float glowIntensity)
    {
        SpriteRenderer.material.SetColor("_OutlineColor", color);
        SpriteRenderer.material.SetFloat("_GlowIntensity", glowIntensity);
    }

    public void setOutlineColorOrg()
    {
        SpriteRenderer.material.SetColor("_OutlineColor", playerOutlineColorOrg);
        SpriteRenderer.material.SetFloat("_GlowIntensity", 1f);
    }

    public void setOutlineColorEmitable()
    {
        SpriteRenderer.material.SetColor("_OutlineColor", playerOutlineEmitable);
        SpriteRenderer.material.SetFloat("_GlowIntensity", 3f);
    }
}

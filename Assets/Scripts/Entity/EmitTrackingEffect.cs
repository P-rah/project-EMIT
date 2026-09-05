using UnityEngine;

public class EmitTrackingEffect : MonoBehaviour
{
    private Transform Transform;
    private SpriteRenderer Renderer;

    private void OnEnable()
    {
        Transform = GetComponent<Transform>();
        Renderer = GetComponent<SpriteRenderer>();
    }

    public void setGhost(Sprite sprite , Vector2 position, Vector2 facingDir)
    {
        Transform.position = position;
        Renderer.sprite = sprite;
        Renderer.flipX = facingDir.x > 0 ? false : true;
    }
}

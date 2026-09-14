using UnityEngine;

public class EmitTrackingEffect : MonoBehaviour
{
    private Transform Transform;
    private SpriteRenderer Renderer;
    private bool endMove = false;

    private void Awake()
    {
        Transform = GetComponent<Transform>();
        Renderer = GetComponent<SpriteRenderer>();
    }

    public void setGhost(Sprite sprite , Vector2 position, Vector2 facingDir)
    {
        endMove = false;
        Transform.position = position;
        Renderer.sprite = sprite;
        Renderer.flipX = facingDir.x > 0 ? false : true;
    }

    public Vector2 getPos() { return Transform.position; }

    public Sprite getSpirte() { return Renderer.sprite; }

    public void EndMove() { endMove = true; }

    public bool isEndMove() { return endMove; }
}

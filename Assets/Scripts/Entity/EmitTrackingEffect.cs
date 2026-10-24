using UnityEngine;

public class EmitTrackingEffect : MonoBehaviour
{
    private SpriteRenderer renderer;
    private bool endMove = false;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // 고스트 위치 확정하기
    public void setGhost(Sprite sprite , Vector2 position, Vector2 facingDir, float ghostAlpha)
    {
        endMove = false;
        transform.position = position;
        renderer.sprite = sprite;
    
        Color ghostColor = Color.yellow;
        ghostColor.a = ghostAlpha;
        renderer.color = ghostColor;
    
        renderer.flipX = facingDir.x > 0 ? false : true;
    }

    public Vector2 getPos() { return transform.position; }

    public Sprite getSpirte() { return renderer.sprite; }

    public void EndMove() { endMove = true; }

    public bool isEndMove() { return endMove; }
}

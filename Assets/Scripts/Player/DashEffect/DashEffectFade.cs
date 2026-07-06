using UnityEngine;

public class DashEffectFade : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color color;
    private float fadeSpeed;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(Transform playerTransform, Color effectColor, float speed)
    {
        transform.position = playerTransform.position;
        transform.rotation = playerTransform.rotation;
        transform.localScale = playerTransform.localScale;

        color = effectColor;
        sr.color = color;
        fadeSpeed = speed;
    }

    void Update()
    {
        color.a -= fadeSpeed * Time.deltaTime;
        sr.color = color;
        if (color.a <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}

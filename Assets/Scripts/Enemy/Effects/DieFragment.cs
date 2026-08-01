using UnityEngine;
using System.Collections;

public class DieFragment : MonoBehaviour
{
    [Header("Æø¹ß Èû ¼³Á¤")]
    public float minForce = 5f;
    public float maxForce = 10f;
    public float maxTorque = 50f;

    [Header("ÆäÀÌµå¾Æ¿ô ¼³Á¤")]
    public float lifeTime = 2f;
    public float fadeDuration = 1f;

    private struct TransformData
    {
        public Transform childTransform;
        public Vector3 originalLocalPosition;
        public Quaternion originalLocalRotation;
    }
    private TransformData[] fragments;
    private Rigidbody2D[] rigidbodies;
    private SpriteRenderer[] renderers;

    private void Awake()
    {
        int childCount = transform.childCount;
        fragments = new TransformData[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            fragments[i] = new TransformData
            {
                childTransform = child,
                originalLocalPosition = child.localPosition,
                originalLocalRotation = child.localRotation
            };
        }

        rigidbodies = GetComponentsInChildren<Rigidbody2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        ResetTransform();

        ResetAlpha();

        Explode();

        StartCoroutine(FadeOutAndDisable());
    }

    private void Explode()
    {
        foreach (Transform child in transform)
        {
            Rigidbody2D rb = child.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                float randomX = Random.Range(-1f, 1f);
                float randomY = Random.Range(0.5f, 1.5f);
                Vector2 direction = new Vector2(randomX, randomY).normalized;

                float force = Random.Range(minForce, maxForce);

                rb.AddForce(direction * force, ForceMode2D.Impulse);

                float torque = Random.Range(-maxTorque, maxTorque);
                rb.AddTorque(torque);

            }
        }
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return new WaitForSeconds(lifeTime);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            foreach (SpriteRenderer sr in renderers)
            {
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }
            yield return null;
        }

        DisableDieFrag();
    }

    private void ResetTransform()
    {
        for (int i = 0; i < fragments.Length; i++)
        {
            fragments[i].childTransform.localPosition = fragments[i].originalLocalPosition;
            fragments[i].childTransform.localRotation = fragments[i].originalLocalRotation;
        }
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].linearVelocity = Vector2.zero;
            rigidbodies[i].angularVelocity = 0f;
        }
    }

    private void ResetAlpha()
    {
        if (renderers == null) return;

        foreach (SpriteRenderer sr in renderers)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }
    }

    private void DisableDieFrag()
    {
        EffectManager.Instance.ReturnDeathEffect(this.gameObject);
    }
}

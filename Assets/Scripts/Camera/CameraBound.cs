using Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(TilemapCollider2D), typeof(PolygonCollider2D))]
public class CameraBound : MonoBehaviour
{
    private void Start()
    {
        if (!TryGetComponent(out TilemapCollider2D tilemapCollider)
            || !TryGetComponent(out PolygonCollider2D boundingShape))
        {
            Debug.LogError(
                "CameraBound requires both a TilemapCollider2D and a PolygonCollider2D on the same GameObject.",
                this);
            return;
        }

        Bounds sourceBounds = tilemapCollider.bounds;

        // A TilemapCollider2D that is merged into a CompositeCollider2D has no bounds of its own.
        if (!HasArea(sourceBounds)
            && TryGetComponent(out CompositeCollider2D compositeCollider))
        {
            sourceBounds = compositeCollider.bounds;
        }

        if (!HasArea(sourceBounds))
        {
            Debug.LogError(
                "CameraBound could not create bounds because neither the TilemapCollider2D nor its CompositeCollider2D has an area.",
                this);
            return;
        }

        Vector3 localMin3 = transform.InverseTransformPoint(new Vector3(
            sourceBounds.min.x,
            sourceBounds.min.y,
            transform.position.z));
        Vector3 localMax3 = transform.InverseTransformPoint(new Vector3(
            sourceBounds.max.x,
            sourceBounds.max.y,
            transform.position.z));
        Vector2 localMin = new Vector2(localMin3.x, localMin3.y);
        Vector2 localMax = new Vector2(localMax3.x, localMax3.y);

        Vector2 offset = boundingShape.offset;
        boundingShape.points = new[]
        {
            localMin - offset,
            new Vector2(localMin.x, localMax.y) - offset,
            localMax - offset,
            new Vector2(localMax.x, localMin.y) - offset
        };

        foreach (CinemachineConfiner2D confiner in FindObjectsByType<CinemachineConfiner2D>(
                     FindObjectsInactive.Include))
        {
            if (confiner.m_BoundingShape2D == boundingShape)
                confiner.InvalidateCache();
        }
    }

    private static bool HasArea(Bounds bounds)
    {
        return bounds.size.x > 0f && bounds.size.y > 0f;
    }
}

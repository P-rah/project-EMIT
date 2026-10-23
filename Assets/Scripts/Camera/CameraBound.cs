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

        const float thickness = 1f;
        Vector2 center = (localMin + localMax) * 0.5f;
        Vector2 size = localMax - localMin;

        AddWall(new Vector2(localMin.x - thickness / 2f, center.y),
            new Vector2(thickness, size.y + thickness * 2f));
        AddWall(new Vector2(localMax.x + thickness / 2f, center.y),
            new Vector2(thickness, size.y + thickness * 2f));
        AddWall(new Vector2(center.x, localMin.y - thickness / 2f),
            new Vector2(size.x, thickness));
        AddWall(new Vector2(center.x, localMax.y + thickness / 2f),
            new Vector2(size.x, thickness));

        foreach (CinemachineConfiner2D confiner in FindObjectsByType<CinemachineConfiner2D>(
                     FindObjectsInactive.Include))
        {
            if (confiner.m_BoundingShape2D == boundingShape)
                confiner.InvalidateCache();
        }
    }

    private void AddWall(Vector2 center, Vector2 size)
    {
        BoxCollider2D wall = gameObject.AddComponent<BoxCollider2D>();
        wall.offset = center;
        wall.size = size;
        wall.isTrigger = false;
        wall.compositeOperation = Collider2D.CompositeOperation.None;
    }

    private static bool HasArea(Bounds bounds)
    {
        return bounds.size.x > 0f && bounds.size.y > 0f;
    }
}

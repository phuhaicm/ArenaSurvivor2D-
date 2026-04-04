using UnityEngine;

public static class BoundsEdgePositionUtility
{
    public static Vector2 RandomPointOnEdge(Bounds bounds, float padding)
    {
        float minX = bounds.min.x + padding;
        float maxX = bounds.max.x - padding;
        float minY = bounds.min.y + padding;
        float maxY = bounds.max.y - padding;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: return new Vector2(Random.Range(minX, maxX), maxY);
            case 1: return new Vector2(Random.Range(minX, maxX), minY);
            case 2: return new Vector2(minX, Random.Range(minY, maxY));
            default: return new Vector2(maxX, Random.Range(minY, maxY));
        }
    }
}
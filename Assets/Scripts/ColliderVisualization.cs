#if UNITY_EDITOR
using UnityEngine;
[ExecuteAlways]
public class ColliderVisualizer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Collider2D[] colliders = FindObjectsOfType<Collider2D>();

        Color defaultColor = Color.green;

        foreach(Collider2D col in colliders)
        {
            if (col is BoxCollider2D box)
            {
                Gizmos.color = defaultColor;
                Gizmos.DrawWireCube((Vector3)box.bounds.center, (Vector3)box.size);
            } else if(col is CircleCollider2D circle)
            {
                Gizmos.color = defaultColor;
                Gizmos.DrawWireSphere((Vector3)circle.bounds.center, circle.radius);
            } else if(col is PolygonCollider2D poly)
            {
                Gizmos.color = defaultColor;
                Vector2[] points = poly.points;
                for(int i = 0; i < points.Length; i++)
                {
                    Vector2 p1 = poly.transform.TransformPoint(points[i]);
                    Vector2 p2 = poly.transform.TransformPoint(points[(i + 1) % points.Length]);
                    Gizmos.DrawLine(p1, p2);
                }
            } else if(col is CapsuleCollider2D capsule)
            {
                Gizmos.color = defaultColor;
                Gizmos.DrawWireCube((Vector3)capsule.bounds.center, (Vector3)capsule.bounds.size);
            } else if(col is CompositeCollider2D composite)
            {
                Gizmos.color = Color.blue;
                int pathCount = composite.pathCount;
                Vector2[] pathPoints = new Vector2[composite.pointCount];

                for(int i = 0; i < pathCount; i++)
                {
                    int pointCount = composite.GetPath(i, pathPoints);

                    for(int j = 0; j < pointCount; j++)
                    {
                        Vector3 p1 = composite.transform.TransformPoint(pathPoints[j]);
                        Vector3 p2 = composite.transform.TransformPoint(pathPoints[(j + 1) % pointCount]);
                        Gizmos.DrawLine(p1, p2);
                    }
                }
            }
        }
    }
}
#endif
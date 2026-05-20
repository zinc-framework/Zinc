using System.Numerics;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Internal.Box2D;
using Zinc.Internal.Sokol;

namespace Zinc;

public static class Collision
{
    public static readonly Transform2D Identity = new Transform2D(0, 0, 0);

    public class Transform2D(int x, int y, float r)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        public float R { get; } = r;

        public static implicit operator b2Transform(Transform2D d) =>
            new b2Transform()
            {
                p = new b2Vec2 { x = d.X, y = d.Y },
                q = new b2Rot { c = MathF.Cos(d.R), s = MathF.Sin(d.R) }
            };
    }

    private static b2Transform IdentityXf => new b2Transform
    {
        p = new b2Vec2 { x = 0, y = 0 },
        q = new b2Rot { c = 1f, s = 0f }
    };

    public static bool CheckCollision(SceneEntity e1, SceneEntity e2)
    {
        if (e1.ECSEntity.Has<Collider>() && e2.ECSEntity.Has<Collider>())
        {
            return CheckCollision(e1.ID, e1.ECSEntity.Get<Collider>(), e2.ID, e2.ECSEntity.Get<Collider>());
        }
        Console.WriteLine("entites don't have colliders for collision checking");
        return false;
    }

    public static bool CheckCollision(int entityA, Collider a, int entityB, Collider b)
    {
        // Branch by collider mode. Two-point pairs never collide; point-vs-polygon
        // goes through b2PointInPolygon (no hull computation, so it's safe for any
        // polygon size); polygon-vs-polygon uses b2CollidePolygons as before.
        if (a.IsPoint && b.IsPoint) return false;
        if (a.IsPoint) return PointInsidePolygon(GetEntityPosition(entityA), entityB, b);
        if (b.IsPoint) return PointInsidePolygon(GetEntityPosition(entityB), entityA, a);

        var ap = Utils.GetColliderBounds(entityA, a);
        var bp = Utils.GetColliderBounds(entityB, b);
        if (!ap.Valid || !bp.Valid) return false;
        unsafe
        {
            fixed (b2Polygon* a_ptr = &ap.poly, b_ptr = &bp.poly)
            {
                b2Manifold m = Box2D.b2CollidePolygons(a_ptr, IdentityXf, b_ptr, IdentityXf);
                return m.pointCount > 0;
            }
        }
    }

    private static Vector2 GetEntityPosition(int entityID)
    {
        return (Vector2)Engine.GetEntity(entityID).ECSEntity.Get<Position>();
    }

    // Point-vs-polygon test using Box2D's b2PointInPolygon. Sidesteps the
    // b2ComputeHull / b2ValidateHull path entirely so it stays safe even when
    // the polygon's world-space corners would collapse under LINEAR_SLOP.
    public static bool PointInsidePolygon(Vector2 point, int polygonEntityID, Collider c)
    {
        var poly = Utils.GetColliderBounds(polygonEntityID, c);
        if (!poly.Valid) return false;
        unsafe
        {
            fixed (b2Polygon* p_ptr = &poly.poly)
            {
                return Box2D.b2PointInPolygon(new b2Vec2 { x = point.X, y = point.Y }, p_ptr) != 0;
            }
        }
    }

    public static (Vector2? a, Vector2? b) GetClosestPoints(Entity e1, Entity e2)
    {
        if (e1.ECSEntity.Has<Collider>() && e2.ECSEntity.Has<Collider>())
        {
            return GetClosestPoints(e1.ID, e1.ECSEntity.Get<Collider>(), e2.ID, e2.ECSEntity.Get<Collider>());
        }
        Console.WriteLine("entites don't have colliders for collision checking");
        return (null, null);
    }

    // Closest point on the polygon to an external world-space point. Uses
    // b2ShapeDistance with a single-vertex proxy for the input point.
    public static Vector2 ClosestPointOnPolygon(Vector2 point, int polygonEntityID, Collider c)
    {
        var poly = Utils.GetColliderBounds(polygonEntityID, c);
        if (!poly.Valid) return point;
        unsafe
        {
            b2DistanceInput input = default;
            input.proxyA.points[0] = new b2Vec2 { x = point.X, y = point.Y };
            input.proxyA.count = 1;
            input.proxyA.radius = 0f;

            for (int i = 0; i < poly.poly.count; i++) input.proxyB.points[i] = poly.poly.vertices[i];
            input.proxyB.count = poly.poly.count;
            input.proxyB.radius = poly.poly.radius;

            input.transformA = IdentityXf;
            input.transformB = IdentityXf;
            input.useRadii = 0;

            b2SimplexCache cache = default;
            b2DistanceOutput output = Box2D.b2ShapeDistance(&input, &cache, null, 0);
            return new Vector2(output.pointB.x, output.pointB.y);
        }
    }

    public static (Vector2 a, Vector2 b) GetClosestPoints(int entityA, Collider a, int entityB, Collider b)
    {
        if (a.IsPoint && b.IsPoint)
            return (GetEntityPosition(entityA), GetEntityPosition(entityB));
        if (a.IsPoint)
        {
            var pt = GetEntityPosition(entityA);
            return (pt, ClosestPointOnPolygon(pt, entityB, b));
        }
        if (b.IsPoint)
        {
            var pt = GetEntityPosition(entityB);
            return (ClosestPointOnPolygon(pt, entityA, a), pt);
        }

        var ap = Utils.GetColliderBounds(entityA, a);
        var bp = Utils.GetColliderBounds(entityB, b);
        if (!ap.Valid || !bp.Valid) return (Vector2.Zero, Vector2.Zero);
        unsafe
        {
            b2DistanceInput input = default;
            // Use the polygon vertices directly as the distance proxies. Box2D's b2ShapeProxy
            // is a flat list of vertices + radius — same shape as a polygon's hull.
            for (int i = 0; i < ap.poly.count; i++) input.proxyA.points[i] = ap.poly.vertices[i];
            input.proxyA.count = ap.poly.count;
            input.proxyA.radius = ap.poly.radius;

            for (int i = 0; i < bp.poly.count; i++) input.proxyB.points[i] = bp.poly.vertices[i];
            input.proxyB.count = bp.poly.count;
            input.proxyB.radius = bp.poly.radius;

            input.transformA = IdentityXf;
            input.transformB = IdentityXf;
            input.useRadii = 0;

            b2SimplexCache cache = default;
            b2DistanceOutput output = Box2D.b2ShapeDistance(&input, &cache, null, 0);
            return (new Vector2(output.pointA.x, output.pointA.y),
                    new Vector2(output.pointB.x, output.pointB.y));
        }
    }

    public static CollisionInfo GetCollisionInfo(SceneEntity e1, SceneEntity e2)
    {
        if (e1.ECSEntity.Has<Collider>() && e2.ECSEntity.Has<Collider>())
        {
            return GetCollisionInfo(e1.ID, e1.ECSEntity.Get<Collider>(), e2.ID, e2.ECSEntity.Get<Collider>());
        }
        Console.WriteLine("entites don't have colliders for collision checking");
        return null;
    }

    public static CollisionInfo GetCollisionInfo(int entityA, Collider a, int entityB, Collider b)
    {
        // CollisionInfo wraps a b2Manifold which is only produced by polygon
        // collisions. Point pairings should use CheckCollision + GetClosestPoints.
        if (a.IsPoint || b.IsPoint)
        {
            Console.WriteLine("GetCollisionInfo only supports polygon-vs-polygon — use CheckCollision/GetClosestPoints for point colliders");
            return null;
        }
        var ap = Utils.GetColliderBounds(entityA, a);
        var bp = Utils.GetColliderBounds(entityB, b);
        if (!ap.Valid || !bp.Valid) return null;
        unsafe
        {
            fixed (b2Polygon* a_ptr = &ap.poly, b_ptr = &bp.poly)
            {
                b2Manifold m = Box2D.b2CollidePolygons(a_ptr, IdentityXf, b_ptr, IdentityXf);
                return new CollisionInfo(m);
            }
        }
    }

    public record CollisionInfo
    {
        public int Count { get; init; }
        public Vector2 RayFromAToB { get; init; }
        public Vector2 PointA { get; init; }
        public Vector2 PointB { get; init; }
        public CollisionInfo(b2Manifold m)
        {
            Count = m.pointCount;
            // Box2D normal is on the manifold (not per-contact like cute's).
            RayFromAToB = new Vector2(m.normal.x, m.normal.y);
            PointA = new Vector2(m.points[0].point.x, m.points[0].point.y);
            PointB = new Vector2(m.points[1].point.x, m.points[1].point.y);
        }
    }
}

public static class Utils
{
    public static Polygon GetColliderBounds(int entityID, Collider c)
    {
        return new Polygon(GetBounds(entityID, c));
    }

    public static Vector2[] GetBounds(int entityID, Collider c)
    {
        if (Engine.GetEntity(entityID) is Anchor entity)
        {
            var (worldTransform, worldScale) = entity.GetWorldTransform();
            Vector2 pivot = new Vector2(c.Pivot.X * c.Width, c.Pivot.Y * c.Height);

            return new Vector2[]
            {
                Vector2.Transform(new Vector2(-pivot.X            * worldScale.X, -pivot.Y * worldScale.Y), worldTransform),
                Vector2.Transform(new Vector2((c.Width - pivot.X) * worldScale.X, -pivot.Y * worldScale.Y), worldTransform),
                Vector2.Transform(new Vector2((c.Width - pivot.X) * worldScale.X, (c.Height - pivot.Y) * worldScale.Y), worldTransform),
                Vector2.Transform(new Vector2(-pivot.X            * worldScale.X, (c.Height - pivot.Y) * worldScale.Y), worldTransform)
            };
        }

        var pos = (Vector2)Engine.GetEntity(entityID).ECSEntity.Get<Position>();
        return Enumerable.Repeat(pos, 4).ToArray();
    }
}

public class Polygon
{
    public b2Polygon poly;
    // False when the input collapsed to a degenerate hull (e.g. tiny colliders
    // like the 1px cursor + Box2D's LINEAR_SLOP point-welding can reduce 4
    // corners to <3 unique points). Collision queries short-circuit on this so
    // we don't hand a bad b2Polygon to b2MakePolygon and trip its assert.
    public bool Valid;
    public Polygon(Vector2[] points)
    {
        unsafe
        {
            var b2pts = stackalloc b2Vec2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                b2pts[i].x = points[i].X;
                b2pts[i].y = points[i].Y;
            }
            b2Hull hull = Box2D.b2ComputeHull(b2pts, points.Length);
            if (hull.count < 3 || Box2D.b2ValidateHull(&hull) == 0)
            {
                Valid = false;
                return;
            }
            poly = Box2D.b2MakePolygon(&hull, 0f);
            Valid = true;
        }
    }
}

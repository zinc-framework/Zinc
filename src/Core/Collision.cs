using System.Numerics;
using Arch.Core.Extensions;
using Zinc.Core;
using Zinc.Core.ImGUI;
using Zinc.Internal.Cute;
using Zinc.Internal.Sokol;

namespace Zinc;

public static class Collision
{
    //TODO:
    //wrap these:
    //C2.c2Collide();
    //C2.c2Collided();
    public static readonly Transform2D Identity = new Transform2D(0, 0, 0);
    
    public class Transform2D(int x, int y, float r)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        public float R { get; } = r;
        public static implicit operator c2x(Transform2D d) => 
            new c2x()
            {
                p = new (){x = d.X, y = d.Y},
                r = new c2r(){c = MathF.Cos(d.R),s = MathF.Sin(d.R)}
            };
    }

    public static bool CheckCollision(Entity e1, Entity e2)
    {
        if (e1.ECSEntity.Has<Collider>() && e2.ECSEntity.Has<Collider>())
        {
            return CheckCollision(e1.ECSEntity.Get<Collider>(), e1.ECSEntity.Get<Position>(), e2.ECSEntity.Get<Collider>(), e2.ECSEntity.Get<Position>());
        }
        
        Console.WriteLine("entites don't have colliders for collision checking");
        return false;
    }

    public static bool CheckCollision(Collider a, Position app, Collider b, Position bpp)
    {
        var ap = Utils.GetColliderBounds(a,app);
        var bp = Utils.GetColliderBounds(b,bpp);
        var c = 0;
        unsafe
        {
            fixed (c2Poly* a_ptr = &ap.poly, b_ptr = &bp.poly)
            {
                c = C2.c2PolytoPoly(a_ptr, null, b_ptr, null);
            }
        }
        return c > 0;
    }
    
    public static (Vector2? a, Vector2? b) GetClosestPoints(Entity e1, Entity e2)
    {
        if (e1.ECSEntity.Has<Collider>() && e2.ECSEntity.Has<Collider>())
        {
            return GetClosestPoints(e1.ECSEntity.Get<Collider>(), e1.ECSEntity.Get<Position>(), e2.ECSEntity.Get<Collider>(), e2.ECSEntity.Get<Position>());
        }
        
        Console.WriteLine("entites don't have colliders for collision checking");
        return (null,null);
    }
    
    public static (Vector2 a, Vector2 b) GetClosestPoints(Collider a, Position app, Collider b, Position bpp)
    {
        var ap = Utils.GetColliderBounds(a,app);
        var bp = Utils.GetColliderBounds(b,bpp);
        c2v outA = default;
        c2v outB = default;
        unsafe
        {
            fixed (c2Poly* a_ptr = &ap.poly, b_ptr = &bp.poly)
            {
                C2.c2GJK(a_ptr, C2_TYPE.C2_TYPE_POLY, null, b_ptr, C2_TYPE.C2_TYPE_POLY, null, &outA, &outB, 0, null, null);
            }
        }
        return (outA.ToVector2(),outB.ToVector2());
    }
    
    public static CollisionInfo GetCollisionInfo(Entity e1, Entity e2)
    {
        if (e1.ECSEntity.Has<Collider>() && e2.ECSEntity.Has<Collider>())
        {
            return GetCollisionInfo(e1.ECSEntity.Get<Collider>(), e1.ECSEntity.Get<Position>(), e2.ECSEntity.Get<Collider>(), e2.ECSEntity.Get<Position>());
        }
        
        Console.WriteLine("entites don't have colliders for collision checking");
        return null;
    }
    public static CollisionInfo GetCollisionInfo(Collider a, Position app, Collider b, Position bpp)
    {
        c2Manifold manifold = default;
        var ap = Utils.GetColliderBounds(a,app);
        var bp = Utils.GetColliderBounds(b,bpp);
        unsafe
        {
            fixed (c2Poly* a_ptr = &ap.poly, b_ptr = &bp.poly)
            {
                C2.c2PolytoPolyManifold(a_ptr, null, b_ptr, null ,&manifold);
            }
        }
        return new CollisionInfo(manifold);
    }

    public record CollisionInfo
    {
        public int Count { get; init; }
        public Vector2 RayFromAToB { get; init; }
        public Vector2 PointA { get; init; }
        public Vector2 PointB { get; init; }
        public CollisionInfo(c2Manifold m)
        {
            Count = m.count;
            RayFromAToB = new(m.n.x,m.n.y);
            PointA = m.contact_points[0].ToVector2();
            PointB = m.contact_points[1].ToVector2();
        }
    }
}

public static class Utils
{
    public static Polygon GetColliderBounds(Collider c, Position entityPosition)
    {
        return new Polygon(4,GetBounds(c,entityPosition));
    }
    
    public static Vector2[] GetBounds(Collider c, Position entityPosition)
    {
        //the position of the entity IS the world space pivot
        var pos = new Vector2(entityPosition.X, entityPosition.Y);
        
        //get collider points
        var colliderRoot = new Vector2(c.X - entityPosition.PivotX, c.Y - entityPosition.PivotY);
        var topLeft = pos.Translate(colliderRoot);
        var topRight = pos.Translate(new Vector2(colliderRoot.X + c.Width, colliderRoot.Y));
        var bottomLeft = pos.Translate(new Vector2(colliderRoot.X + c.Width, colliderRoot.Y + c.Height));
        var bottomRight = pos.Translate(new Vector2(colliderRoot.X, colliderRoot.Y + c.Height));
        Vector2[] pts = new Vector2[]
        {
            //working
            topLeft.Transform(entityPosition.Rotation, entityPosition.ScaleX, entityPosition.ScaleY,pivot:pos),
            topRight.Transform(entityPosition.Rotation, entityPosition.ScaleX, entityPosition.ScaleY,pivot:pos),
            bottomLeft.Transform(entityPosition.Rotation, entityPosition.ScaleX, entityPosition.ScaleY,pivot:pos),
            bottomRight.Transform(entityPosition.Rotation, entityPosition.ScaleX, entityPosition.ScaleY,pivot:pos),
        };

        // Assuming Transform method correctly applies the pivot and translation.
        return pts;
    }




}

public class Circle(float x, float y, float radius)
{
    private c2Circle collider = new()
    {
        p = new() { x = x, y = y }, 
        r = radius
    };
}

public class AABB(float minX, float minY, float maxX, float maxY)
{
    private c2AABB collider = new()
    {
        min = new c2v { x = minX, y = minY },
        max = new c2v { x = maxX, y = maxY }
    };
}

//Capsule: This struct represents a capsule shape, defined by two endpoints (a and b) and a radius r.
public class Capsule(float ax, float ay, float bx, float by, float radius)
{
    private c2Capsule collider = new()
    {
        a = new c2v { x = ax, y = ay },
        b = new c2v { x = bx, y = by },
        r = radius
    };
}

/*
 * c2v p;   // position
   c2v d;   // direction (normalized)
   float t; // distance along d from position p to find endpoint of ray
 */
public class Ray(float px, float py, float dx, float dy, float t)
{
    private c2Ray collider = new()
    {
        p = new c2v { x = px, y = py },
        d = new c2v { x = dx, y = dy },
        t = t
    };
}

/*
 * 	float t; // time of impact
   c2v n;   // normal of surface at impact (unit length)
 */
// public class Raycast(float t, float nx, float ny)
// {
//     private c2Raycast result = new()
//     {
//         t = t,
//         n = new c2v { x = nx, y = ny }
//     };
// }

public class Polygon
{
    public c2Poly poly;
    public Polygon(int count, Vector2[] points)
    {
        poly = default;
        poly.count = count;
        for (int i = 0; i < count; i++)
        {
            poly.verts[i] = points[i].ToC2V();
        }

        unsafe
        {
            fixed(c2Poly* ptr = &poly)
            {
                C2.c2MakePoly(ptr);
            }
        }
    }
}




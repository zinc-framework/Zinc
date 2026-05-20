using System.Numerics;
using Zinc.Internal.Box2D;

namespace Zinc;

// Thin C# wrapper around box2d v3.1's C API. The framework owns one PhysicsWorld
// (Engine.PhysicsWorld); user code mostly creates PhysicsBody instances and reads
// their Position back into entity transforms.
//
// Coordinate space: Zinc draws in screen-space (Y-down). Box2D itself doesn't care
// about handedness — we just choose a gravity vector that points "down" on screen.
// The default below (0, 980) pulls bodies toward +Y at ~1g for pixel-scale worlds.
public sealed class PhysicsWorld
{
    public b2WorldId Id { get; private set; }
    public int SubStepCount { get; set; } = 4;
    public float FixedTimeStep { get; set; } = 1f / 60f;

    // Hard ceiling on per-frame physics time. Gaffer-on-Games "Fix Your Timestep":
    // if a frame stalls (window drag, GC pause, debugger break), we'd otherwise try
    // to play catch-up forever, queueing input behind the simulation. Cap at 250ms
    // so at worst we run ~15 steps per frame after a spike, then resync.
    private const float MaxFrameTime = 0.25f;

    private float accumulator;

    // 0..1: how far through the *next* fixed step we are after consuming this frame.
    // Renderers that want sub-step smoothness should lerp between prev/current state
    // by this value. Demos that read body positions directly will see them snap to
    // 60Hz, which is fine for the current set.
    public float Alpha { get; private set; }

    public PhysicsWorld(Vector2? gravity = null)
    {
        unsafe
        {
            b2WorldDef def = Box2D.b2DefaultWorldDef();
            var g = gravity ?? new Vector2(0f, 980f);
            def.gravity = new b2Vec2 { x = g.X, y = g.Y };
            Id = Box2D.b2CreateWorld(&def);
        }
    }

    // Caller passes raw (unfiltered) frame duration. We clamp + fixed-step so
    // simulation rate is decoupled from render rate and survives stalls without
    // spiraling.
    public void Update(double frameTime)
    {
        float ft = (float)frameTime;
        if (ft > MaxFrameTime) ft = MaxFrameTime;

        accumulator += ft;
        while (accumulator >= FixedTimeStep)
        {
            Box2D.b2World_Step(Id, FixedTimeStep, SubStepCount);
            accumulator -= FixedTimeStep;
        }
        Alpha = accumulator / FixedTimeStep;
    }

    public void Destroy()
    {
        Box2D.b2DestroyWorld(Id);
    }

    public PhysicsBody CreateStaticBody(Vector2 position, float angleRadians = 0f)
        => CreateBody(b2BodyType.b2_staticBody, position, angleRadians);

    public PhysicsBody CreateDynamicBody(Vector2 position, float angleRadians = 0f)
        => CreateBody(b2BodyType.b2_dynamicBody, position, angleRadians);

    public PhysicsBody CreateKinematicBody(Vector2 position, float angleRadians = 0f)
        => CreateBody(b2BodyType.b2_kinematicBody, position, angleRadians);

    private PhysicsBody CreateBody(b2BodyType type, Vector2 position, float angle)
    {
        unsafe
        {
            b2BodyDef bd = Box2D.b2DefaultBodyDef();
            bd.type = type;
            bd.position = new b2Vec2 { x = position.X, y = position.Y };
            bd.rotation = new b2Rot { c = MathF.Cos(angle), s = MathF.Sin(angle) };
            b2BodyId id = Box2D.b2CreateBody(Id, &bd);
            return new PhysicsBody(id);
        }
    }
}

public sealed class PhysicsBody
{
    public b2BodyId Id { get; }

    internal PhysicsBody(b2BodyId id)
    {
        Id = id;
    }

    public Vector2 Position
    {
        get
        {
            var p = Box2D.b2Body_GetPosition(Id);
            return new Vector2(p.x, p.y);
        }
    }

    public float Angle
    {
        get
        {
            var r = Box2D.b2Body_GetTransform(Id).q;
            return MathF.Atan2(r.s, r.c);
        }
    }

    // Teleport — bypasses physics, useful for resetting bodies or one-shot placement.
    public void Set(Vector2 position, float angleRadians)
    {
        Box2D.b2Body_SetTransform(
            Id,
            new b2Vec2 { x = position.X, y = position.Y },
            new b2Rot { c = MathF.Cos(angleRadians), s = MathF.Sin(angleRadians) });
    }

    // Continuous force application — call every step that the force is active.
    public void AddForce(Vector2 force, bool wake = true)
    {
        Box2D.b2Body_ApplyForceToCenter(Id, new b2Vec2 { x = force.X, y = force.Y }, (byte)(wake ? 1 : 0));
    }

    public void AddForce(Vector2 force, Vector2 worldPoint, bool wake = true)
    {
        Box2D.b2Body_ApplyForce(
            Id,
            new b2Vec2 { x = force.X, y = force.Y },
            new b2Vec2 { x = worldPoint.X, y = worldPoint.Y },
            (byte)(wake ? 1 : 0));
    }

    public b2ShapeId AddBoxShape(float width, float height, float density = 1f, float friction = 0.6f)
    {
        unsafe
        {
            b2ShapeDef sd = Box2D.b2DefaultShapeDef();
            sd.density = density;
            sd.material.friction = friction;
            b2Polygon box = Box2D.b2MakeBox(width * 0.5f, height * 0.5f);
            return Box2D.b2CreatePolygonShape(Id, &sd, &box);
        }
    }

    // Add a polygon shape whose vertices are given in world space. Box2D wants
    // body-local vertices, so we offset by the body's current position.
    public b2ShapeId AddPolygonShapeWorldSpace(Vector2[] worldPoints, float density = 1f, float friction = 0.6f)
    {
        unsafe
        {
            var bodyPos = Box2D.b2Body_GetPosition(Id);
            var local = stackalloc b2Vec2[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                local[i].x = worldPoints[i].X - bodyPos.x;
                local[i].y = worldPoints[i].Y - bodyPos.y;
            }
            b2Hull hull = Box2D.b2ComputeHull(local, worldPoints.Length);
            b2Polygon poly = Box2D.b2MakePolygon(&hull, 0f);

            b2ShapeDef sd = Box2D.b2DefaultShapeDef();
            sd.density = density;
            sd.material.friction = friction;
            return Box2D.b2CreatePolygonShape(Id, &sd, &poly);
        }
    }
}

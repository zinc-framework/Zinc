using System.Numerics;
using Zinc.Core;

namespace Zinc;

public record struct ParticleEmitterComponent : IComponent
{
    public int MaxParticles {get; private set;} = 1000;
    public ParticleEmitterConfig Config;
    public bool[] Active;
    public Vector2[] SpawnLocation;
    public Vector2[] Position;
    public Vector2[] Velocity;
    public float[] Mass;
    public double[] Age;
    public int Count = 0;
    public double Accumulator = 0;
    public ParticleEmitterComponent(int maxParticles, ParticleEmitterConfig? config = null)
    {
        MaxParticles = maxParticles;
        Config = config ?? ParticleEmitterConfig.DefaultConfig;
        Active = new bool[maxParticles];
        Position = new Vector2[maxParticles];
        Velocity = new Vector2[maxParticles];
        // Acceleration = new Vector2[maxParticles];
        SpawnLocation = new Vector2[maxParticles];
        Mass = new float[maxParticles];
        Age = new double[maxParticles];   
        Accumulator = Config.EmissionRate;
    }
    public void Resolve(int index, double dt, ref Vector2 pos, ref float width, ref float height, ref float rotation, ref Color color)
    {
        double sampleTime = Age[index] / Config.Lifespan;
        Velocity[index] += ((Config.Gravity * Mass[index]) / Mass[index]) * (float)dt;
        Position[index] += Velocity[index] * (float)dt;

        pos = Position[index];

        width = SampleTransition(Config.Width, sampleTime);
        height = SampleTransition(Config.Height, sampleTime);
        rotation = SampleTransition(Config.Rotation, sampleTime);
        color = SampleColorTransition(Config.Color, sampleTime);
    }

    public void InitParticle(int i, Vector2 spawnLocation)
    {
        Active[i] = true;
        Position[i] = Vector2.Zero;
        SpawnLocation[i] = spawnLocation;
        Velocity[i] = Config.InitialEmissionDirectionFunc() * Config.InitialSpeedFunc();
        Mass[i] = Config.InitialMassFunc();
        Age[i] = 0;
    }

    private float SampleTransition(Transition<float> transition, double sampleTime)
    {
        float sample = (float)transition.Sample(sampleTime);
        return Quick.MapF(sample, 0f, 1f, transition.StartValue, transition.TargetValue);
    }

    private Color SampleColorTransition(Transition<Color> transition, double sampleTime)
    {
        float sample = (float)transition.Sample(sampleTime);
        return new Color(
            Quick.MapF(sample, 0f, 1f, transition.StartValue.R, transition.TargetValue.R),
            Quick.MapF(sample, 0f, 1f, transition.StartValue.G, transition.TargetValue.G),
            Quick.MapF(sample, 0f, 1f, transition.StartValue.B, transition.TargetValue.B),
            Quick.MapF(sample, 0f, 1f, transition.StartValue.A, transition.TargetValue.A)
        );
    }

    
}

public class ParticleEmitterConfig
{
    public ParticlePrimitiveType Type; 
    public bool EmitOnce = false;
    public bool HasEmit = false;
    public float EmissionRate;
    public float Lifespan;
    public Vector2 Gravity;
    public Func<Vector2> InitialEmissionDirectionFunc;
    public Func<float> InitialMassFunc;
    public Func<float> InitialSpeedFunc;
    public Transition<float> Width;
    public Transition<float> Height;
    public Transition<Color> Color;
    public Transition<float> Rotation;
    public enum ParticlePrimitiveType
    {
        Rectangle,
        Line,
        LineStrip,
        Triangle,
        // TriangleStrip looks like shit
    }

    public static readonly ParticleEmitterConfig DefaultConfig = new ParticleEmitterConfig(){
        EmitOnce = false,
        HasEmit = false,
        Type = ParticlePrimitiveType.Rectangle,
        Gravity = Quick.StandardGravity,
        InitialSpeedFunc = () => (Quick.RandFloat() + 0.5f) * 500f,
        InitialMassFunc = () => 3f,
        EmissionRate = 200f,
        InitialEmissionDirectionFunc = () => Quick.RandUnitCirclePos(),
        Lifespan = 0.1f,
        Width = new (4,4,Easing.Option.EaseInOutExpo),
        Height = new (4,4,Easing.Option.EaseInOutExpo),
        Color = new (new Color(1,1,1,1),new Color(1,1,1,0),Easing.Option.EaseInOutExpo),
        Rotation = new (0,3 *MathF.PI,Easing.Option.EaseInOutExpo)
    };
}
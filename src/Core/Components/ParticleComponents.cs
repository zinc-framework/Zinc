using System.Numerics;
using Zinc.Core;

namespace Zinc;

public record struct ParticleEmitterComponent : IComponent
{
    public int MaxParticles {get; private set;} = 100;
    public ParticleEmitterConfig Config;
    public bool[] Active;
    public float[] X;
    public float[] Y;
    public double[] Age;
    public int Count = 0;
    public double Accumulator = 0;
    public ParticleEmitterComponent(int maxParticles, ParticleEmitterConfig? config = null)
    {
        Config = config ?? ParticleEmitterConfig.DefaultConfig;
        Active = new bool[maxParticles];
        X = new float[maxParticles];
        Y = new float[maxParticles];
        Age = new double[maxParticles];   
        Accumulator = Config.EmissionRate;
    }
    public void Resolve(int index, ref float x, ref float y,ref float width, ref float height, ref float rotation, ref Color color)
    {
        double sampleTime = Age[index] / Config.Lifespan;
        X[index] += SampleTransition(Config.DX, sampleTime);
        Y[index] += SampleTransition(Config.DY, sampleTime);
        x = X[index];
        y = Y[index];
        width = SampleTransition(Config.Width, sampleTime);
        height = SampleTransition(Config.Height, sampleTime);
        rotation = SampleTransition(Config.Rotation, sampleTime);
        color = SampleColorTransition(Config.Color, sampleTime);
    }

    public void InitParticle(int i)
    {
        Active[i] = true;
        X[i] = 0;
        Y[i] = 0;
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
    public float EmissionRate;
    public ParticlePrimitiveType Type; 
    public float Lifespan;
    public Transition<float> DX;
    public Transition<float> DY;
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
        EmissionRate = 3f,
        Type = ParticlePrimitiveType.Rectangle,
        Lifespan = 0.5f,
        DX = new (4,0.1f,Easing.Option.EaseInOutExpo),
        DY = new (4,0.1f,Easing.Option.EaseInOutExpo),
        Width = new (4,200,Easing.Option.EaseInOutExpo),
        Height = new (4,16,Easing.Option.EaseInOutExpo),
        Color = new (new Color(1,1,1,1),new Color(0,1,1,1),Easing.Option.EaseInOutExpo),
        Rotation = new (0,3 *MathF.PI,Easing.Option.EaseInOutExpo)
    };
}
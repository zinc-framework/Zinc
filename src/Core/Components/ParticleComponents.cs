using System.Numerics;
using Zinc.Core;

namespace Zinc;

public partial record struct ParticleEmitterComponent : IComponent
{
    public class EmitterConfig
    {
        public int MaxParticles;
        public float EmissionRate;
        public ParticleConfig ParticleConfig;
        public EmitterConfig(int maxParticles, float emissionRate, ParticleConfig particleConfig)
        {
            MaxParticles = maxParticles;
            EmissionRate = emissionRate;
            ParticleConfig = particleConfig;
        }
    }

    public EmitterConfig Config { get; set; }
    public List<Particle> Particles = new List<Particle>();
    public double Accumulator = 0f;
    public ParticleEmitterComponent(EmitterConfig c)
    {
        Config = c;
        Particles = new List<Particle>(c.MaxParticles);
    }

    public class ParticleConfig
    {
        public enum ParticlePrimitiveType
        {
            Rectangle,
            Line,
            LineStrip,
            Triangle,
            // TriangleStrip looks like shit
        }
        public float Lifespan;
        public Vector2 EmissionPoint;
        public ParticlePrimitiveType ParticleType;
        public Transition<float> DX;
        public Transition<float> DY;
        public Transition<float> Width;
        public Transition<float> Height;
        public Transition<float> Rotation;
        public Transition<Color> Color;

        public ParticleConfig(Vector2 emissionPoint, ParticlePrimitiveType type, float lifespan, Transition<float> dx, Transition<float> dy, Transition<float> width,
            Transition<float> height, Transition<Color> color, Transition<float> rotation)
        {
            ParticleType = type;
            EmissionPoint = emissionPoint;
            Lifespan = lifespan;
            DX = dx;
            DY = dy;
            Width = width;
            Height = height;
            Color = color;
            Rotation = rotation;
        }

        public ParticleConfig(ParticleConfig c)
        {
            ParticleType = c.ParticleType;
            EmissionPoint = c.EmissionPoint;
            Lifespan = c.Lifespan;
            DX = c.DX;
            DY = c.DY;
            Width = c.Width;
            Height = c.Height;
            Color = c.Color;
            Rotation = c.Rotation;
        }

        public ParticleConfig()
        {
            EmissionPoint = Vector2.Zero;
            ParticleType = DefaultParticleConfig.ParticleType;
            Lifespan = DefaultParticleConfig.Lifespan;
            DX = DefaultParticleConfig.DX;
            DY = DefaultParticleConfig.DY;
            Width = DefaultParticleConfig.Width;
            Height = DefaultParticleConfig.Height;
            Color = DefaultParticleConfig.Color;
            Rotation = DefaultParticleConfig.Rotation;
        }

        public void Resolve(double time,ref float dx, ref float dy, ref float rotation, ref float width,ref float height,ref Color color)
        {
            var sampleTime = time / Lifespan;
            dx = Quick.MapF((float)DX.Sample(sampleTime), 0f, 1f, DX.StartValue, DX.TargetValue);
            dy = Quick.MapF((float)DY.Sample(sampleTime), 0f, 1f, DY.StartValue, DY.TargetValue);
            width = Quick.MapF((float)Width.Sample(sampleTime), 0f, 1f, Width.StartValue, Width.TargetValue);
            height = Quick.MapF((float)Height.Sample(sampleTime), 0f, 1f, Height.StartValue, Height.TargetValue);
            rotation = Quick.MapF((float)Rotation.Sample(sampleTime), 0f, 1f, Rotation.StartValue,
                Rotation.TargetValue);
            ResolveColorTransition(ref color, sampleTime);
        }
        void ResolveColorTransition(ref Color color,double sampleTime)
        {
            float sample = (float)Color.Sample(sampleTime);
            color.A = Quick.MapF(sample,0f,1f,Color.StartValue.internal_color.a,Color.TargetValue.internal_color.a);
            color.R = Quick.MapF(sample,0f,1f,Color.StartValue.internal_color.r,Color.TargetValue.internal_color.r);
            color.G = Quick.MapF(sample,0f,1f,Color.StartValue.internal_color.g,Color.TargetValue.internal_color.g);
            color.B = Quick.MapF(sample,0f,1f,Color.StartValue.internal_color.b,Color.TargetValue.internal_color.b);
        }
    }

    public static readonly ParticleConfig DefaultParticleConfig = new ParticleConfig(Vector2.Zero, 
        ParticleEmitterComponent.ParticleConfig.ParticlePrimitiveType.Rectangle,
        1.5f,
        new (4,0.1f,Easing.Option.EaseInOutExpo),
        new (4,0.1f,Easing.Option.EaseInOutExpo),
        new (4,200,Easing.Option.EaseInOutExpo),
        new (4,16,Easing.Option.EaseInOutExpo),
        new (new Color(1,1,1,1),new Color(0,1,1,1),Easing.Option.EaseInOutExpo),
        new (0,3 *MathF.PI,Easing.Option.EaseInOutExpo));
    public class Particle
    {
        public bool Active = false;
        public float X = 0;
        public float Y = 0;
        public float DX = 0;
        public float DY = 0;
        public float Width = 8;
        public float Height = 8;
        public float Rotation = 0;
        public double Age = 0;
        public Color Color = Palettes.ENDESGA[19];
        public ParticleConfig Config = DefaultParticleConfig;

        public Particle(){}

        public void Reset()
        {
            Age = 0;
            X = 0;
            Y = 0;
            DX = 0;
            DY = 0;
            Rotation = 0;
        }

        public void Resolve()
        {
            Config.Resolve(Age,ref DX, ref DY, ref Rotation, ref Width,ref Height,ref Color);
        }
    }
}
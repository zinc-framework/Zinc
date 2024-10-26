using System.Collections;
using System.Numerics;
using Zinc.Internal.Sokol;

namespace Zinc.Core;

public abstract class Tween<T> : CustomYieldWithValue<T>
{
    public T StartValue;
    public T TargetValue;
    public Func<double,double> TweenFunction; //takes in time and returns a value between 0 and 1
    public Tween(T start, T end, Func<double,double> tweeningFunction)
    {
        StartValue = start;
        TargetValue = end;
        TweenFunction = tweeningFunction;
    }

    public abstract T Sample(double time);
    public T SampleNormalized(double time) => Sample(time/Duration);
    protected override T GetSampleFromTime(double time) => SampleNormalized(time);
}

public class FloatTween : Tween<float>
{
    public FloatTween(float start, float end, Func<double,double> tweeningFunction) : base(start, end, tweeningFunction){}
    public override float Sample(double time)
    {
        return Quick.MapF((float)TweenFunction(time), 0f, 1f, StartValue, TargetValue);
    }
}

public class Vector2Tween : Tween<Vector2>
{
    public Vector2Tween(Vector2 start, Vector2 end, Func<double,double> tweeningFunction) : base(start, end, tweeningFunction){}
    public override Vector2 Sample(double time)
    {
        var sampleTime = (float)TweenFunction(time);
        return new Vector2(
            Quick.MapF(sampleTime, 0f, 1f, StartValue.X, TargetValue.X),
            Quick.MapF(sampleTime, 0f, 1f, StartValue.Y, TargetValue.Y)
        );
    }
}

public class ColorTween : Tween<Color>
{
    public ColorTween(Color start, Color end, Func<double,double> tweeningFunction) : base(start, end, tweeningFunction){}
    public override Color Sample(double time)
    {
        var sampleTime = (float)TweenFunction(time);
        return new Color(
            Quick.MapF(sampleTime, 0f, 1f, StartValue.R, TargetValue.R),
            Quick.MapF(sampleTime, 0f, 1f, StartValue.G, TargetValue.G),
            Quick.MapF(sampleTime, 0f, 1f, StartValue.B, TargetValue.B),
            Quick.MapF(sampleTime, 0f, 1f, StartValue.A, TargetValue.A)
        );
    }
}
namespace Zinc.Core;

public class Transition<T>
{
    public T StartValue;
    public T TargetValue;
    public Easing.Option EasingOption;
    public Transition(T start, T end, Easing.Option option)
    {
        StartValue = start;
        TargetValue = end;
        EasingOption = option;
    }

    public double SampleOverTime(double inputTime, double duration)
    {
        return Sample(Quick.Map(inputTime, 0, duration, 0, 1f));
    }

    public double Sample(double time) => EasingOption switch
    {
        Easing.Option.Linear => Easing.Linear(time),
        Easing.Option.EaseInQuad => Easing.EaseInQuad(time),
        Easing.Option.EaseOutQuad => Easing.EaseOutQuad(time),
        Easing.Option.EaseInOutQuad => Easing.EaseInOutQuad(time),
        Easing.Option.EaseInCubic => Easing.EaseInCubic(time),
        Easing.Option.EaseOutCubic => Easing.EaseOutCubic(time),
        Easing.Option.EaseInOutCubic => Easing.EaseInOutCubic(time),
        Easing.Option.EaseInQuart => Easing.EaseInQuart(time),
        Easing.Option.EaseOutQuart => Easing.EaseOutQuart(time),
        Easing.Option.EaseInOutQuart => Easing.EaseInOutQuart(time),
        Easing.Option.EaseInQuint => Easing.EaseInQuint(time),
        Easing.Option.EaseOutQuint => Easing.EaseOutQuint(time),
        Easing.Option.EaseInOutQuint => Easing.EaseInOutQuint(time),
        Easing.Option.EaseInSine => Easing.EaseInSine(time),
        Easing.Option.EaseOutSine => Easing.EaseOutSine(time),
        Easing.Option.EaseInOutSine => Easing.EaseInOutSine(time),
        Easing.Option.EaseInExpo => Easing.EaseInExpo(time),
        Easing.Option.EaseOutExpo => Easing.EaseOutExpo(time),
        Easing.Option.EaseInOutExpo => Easing.EaseInOutExpo(time),
        Easing.Option.EaseInCirc => Easing.EaseInCirc(time),
        Easing.Option.EaseOutCirc => Easing.EaseOutCirc(time),
        Easing.Option.EaseInOutCirc => Easing.EaseInOutCirc(time),
        Easing.Option.EaseInBack => Easing.EaseInBack(time),
        Easing.Option.EaseOutBack => Easing.EaseOutBack(time),
        Easing.Option.EaseInOutBack => Easing.EaseInOutBack(time),
        Easing.Option.EaseInElastic => Easing.EaseInElastic(time),
        Easing.Option.EaseOutElastic => Easing.EaseOutElastic(time),
        Easing.Option.EaseInOutElastic => Easing.EaseInOutElastic(time),
        Easing.Option.EaseInBounce => Easing.EaseInBounce(time),
        Easing.Option.EaseOutBounce => Easing.EaseOutBounce(time),
        Easing.Option.EaseInOutBounce => Easing.EaseInOutBounce(time)
    };
}
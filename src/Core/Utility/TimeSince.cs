namespace Zinc.Core;

//https://garry.tv/2018/01/16/timesince/
public struct TimeSince
{
    float time;

    public static implicit operator float(TimeSince ts)
    {
        return (float)Engine.Time - ts.time;
    }

    public static implicit operator TimeSince(float ts)
    {
        return new TimeSince { time = (float)Engine.Time - ts };
    }
} 
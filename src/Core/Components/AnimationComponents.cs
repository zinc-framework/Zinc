namespace Zinc;

[Arch.AOT.SourceGenerator.Component]
public record struct SpriteAnimator() : IComponent
{
    public HashSet<Animation> Animations 
    {
        get;
        set
        {
            field = value;
            CurrentAnimation = field.First();  
        } 
    }
    public Animation CurrentAnimation
    {
        get;
        set
        {
            field = value;
            animationIndex = 0;
            AnimationTime = 0f;
        }
    }
    public Rect CurrentAnimationFrame => CurrentAnimation.Frames[animationIndex];
    public double AnimationTime = 0f;
    public bool AnimationStarted = false;
    private int animationIndex = 0;
    public void TickAnimation()
    {
        animationIndex++;
        if (animationIndex >= CurrentAnimation.FrameCount)
        {
            animationIndex = 0;
        }
    }
}
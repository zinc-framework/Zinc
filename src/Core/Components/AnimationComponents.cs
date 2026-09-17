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
            // Force FrameAnimationSystem to push frame 0 of the new clip on its next pass.
            // Otherwise the renderer keeps showing the previous clip's last frame for a full
            // FrameTime and the new clip starts on frame 1.
            AnimationStarted = false;
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
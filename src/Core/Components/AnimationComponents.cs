namespace Zinc;

public record struct SpriteAnimator() : IComponent
{
    private HashSet<Animation> animations;
    public HashSet<Animation> Animations 
    { 
        get => animations; 
        set
        {
            animations = value;
            CurrentAnimation = Animations.First();  
        } 
    }
    private Animation currentAnimation;
    public Animation CurrentAnimation
    {
        get => currentAnimation;
        set
        {
            currentAnimation = value;
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
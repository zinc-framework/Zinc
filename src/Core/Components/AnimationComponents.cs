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
    public Animation CurrentAnimation { get; private set; }
    public Rect CurrentAnimationFrame => CurrentAnimation.Frames[animationIndex];
    public double AnimationTime = 0f;
    public bool AnimationStarted = false;

    public void SetAnimation(string name)
    {
        var anim = Animations.First(x => x.Name == name);
        if (anim != null)
        {
            CurrentAnimation = anim;
            animationIndex = 0;
        }
        else
        {
            Console.WriteLine($"anim {name} not found");
        }
    }

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
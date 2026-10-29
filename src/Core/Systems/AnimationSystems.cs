using Arch.Core;

namespace Zinc;

public abstract class AnimationSystem : DSystem, IPreUpdateSystem
{
    public void PreUpdate(double dt)
    {
        Animate(dt);
    }

    protected abstract void Animate(double dt);
}

public class FrameAnimationSystem : AnimationSystem
{
    QueryDescription query = new QueryDescription().WithAll<ActiveState,SpriteRenderer,SpriteAnimator>();
    protected override void Animate(double dt)
    {
        Engine.ECSWorld.Query(in query, (Arch.Core.Entity e, ref ActiveState act, ref SpriteRenderer r, ref SpriteAnimator a) =>
        {
            if(!act.Active){return;}
            if (a.AnimationStarted == false)
            {
                //we do this to pump the first animation frame to the renderer so we dont render the whole texture first
                a.AnimationStarted = true;
                r.Rect = a.CurrentAnimationFrame;
            }
            // a clip set since the last pass was set during the last frame's update, so it has been playing
            // for this dt too: counted, or every clip would run a frame behind the clock that started it
            a.AnimationTime += dt;
            float frameTime = a.CurrentAnimation.FrameTime;
            if (frameTime <= 0f)
            {
                // no time a frame: a frame a pass, as ever (the catch-up below would never finish)
                a.TickAnimation();
                a.AnimationTime = 0;
            }
            else
            {
                if (a.AnimationTime < frameTime) return;
                // frames change on their exact times: what's over carries into the next frame rather than being
                // dropped (which drew every frame out to the next whole display frame, so a clip ran long), and a
                // dt longer than a frame (a hitch, or frames shorter than the display's) moves on that many frames
                while (a.AnimationTime >= frameTime)
                {
                    a.AnimationTime -= frameTime;
                    a.TickAnimation();
                }
            }
            //note that there is currently no binding glue to imply that SpriteAnimator will work directly on an attached SpriteRenderer
            r.Rect = a.CurrentAnimationFrame;
        });
    }
}
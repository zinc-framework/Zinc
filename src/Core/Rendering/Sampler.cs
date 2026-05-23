using Zinc.Internal.Sokol;

namespace Zinc;

// A GPU texture sampler. The native sg_sampler is created eagerly in the constructor, so a Sampler
// must be constructed when sokol is up (i.e. from Create()/Update(), not a static/module initializer).
// Used by custom-shader texture channels via entity.Material.SetSampler. It's a small readonly value
// (just the handle); default(Sampler) is an invalid handle, like other Zinc native-handle structs.
public readonly record struct Sampler
{
    public sg_sampler Handle { get; }

    // Sensible default: linear filtering, repeat wrap.
    public Sampler() : this(Filter.Linear, Filter.Linear, Wrap.Repeat, Wrap.Repeat) { }

    public unsafe Sampler(Filter min, Filter mag, Wrap wrapU, Wrap wrapV)
    {
        sg_sampler_desc desc = default;
        desc.min_filter = (sg_filter)min;
        desc.mag_filter = (sg_filter)mag;
        desc.wrap_u = (sg_wrap)wrapU;
        desc.wrap_v = (sg_wrap)wrapV;
        Handle = Gfx.make_sampler(&desc);
    }
}

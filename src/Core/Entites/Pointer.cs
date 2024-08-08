using Arch.Core.Extensions;

namespace Zinc;

public partial class Pointer : Entity
{
    public Pointer(bool startEnabled = true) : base(startEnabled,null,false)
    {
        ECSEntity.Add(new Collider(0,0,1,1,Active:true));
    }
}
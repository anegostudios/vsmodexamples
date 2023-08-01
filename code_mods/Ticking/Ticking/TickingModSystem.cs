using Vintagestory.API.Common;

namespace Ticking
{
    public class TickingModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("tickingcounter", typeof(TickingBlockEntity));
        }
    }
}

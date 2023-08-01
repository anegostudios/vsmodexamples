using Vintagestory.API.Common;

namespace MovingBlocks
{
    public class MovingBlocksModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockBehaviorClass("Moving", typeof(Moving));
        }
    }
}

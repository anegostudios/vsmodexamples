using Vintagestory.API.Common;

namespace AdvancedMovingBlocks
{
    public class AdvancedMovingBlocksModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockBehaviorClass("AdvancedMoving", typeof(Moving));
        }
    }
}

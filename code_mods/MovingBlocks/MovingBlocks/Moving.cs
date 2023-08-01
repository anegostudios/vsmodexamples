using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace MovingBlocks{
    class Moving : BlockBehavior
    {
        public Moving(Block block) : base(block)
        {

        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            BlockPos pos = blockSel.Position.AddCopy(blockSel.Face.Opposite);
            if (world.BlockAccessor.GetBlock(pos).IsReplacableBy(block))
            {
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.BlockAccessor.SetBlock(block.BlockId, pos);
            }
            handling = EnumHandling.PreventDefault;
            return true;
        }
    }
}
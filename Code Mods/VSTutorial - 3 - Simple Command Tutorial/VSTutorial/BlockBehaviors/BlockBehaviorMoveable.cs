using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace VSTutorial.BlockBehaviors
{
    internal class BlockBehaviorMoveable : BlockBehavior
    {
        public int distance = 1;
        public bool pull = false;

        public BlockBehaviorMoveable(Block block) : base(block)
        {

        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);
            distance = properties["distance"].AsInt(1);
            pull = properties["pull"].AsBool(false);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            // Find the target position
            BlockPos pos = blockSel.Position.AddCopy(pull && byPlayer.WorldData.EntityControls.Sneak ? blockSel.Face : blockSel.Face.Opposite, distance);

            // Can we place the block there?
            if (world.BlockAccessor.GetBlock(pos).IsReplacableBy(block))
            {
                // Remove the block at the current position and place it at the target position
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                world.BlockAccessor.SetBlock(block.BlockId, pos);
            }
            // Notify the game engine other block behaviors that we handled the players interaction with the block.
            // If we would not set the handling field the player would still be able to place blocks if he has them in hands.
            handling = EnumHandling.PreventDefault;
            return true;
        }

    }
}

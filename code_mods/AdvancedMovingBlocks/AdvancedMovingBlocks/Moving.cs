using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

class Moving : BlockBehavior
{

    public int distance = 1;
    public bool pull = false;

    public Moving(Block block) : base(block)
    {

    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        BlockPos pos = blockSel.Position.AddCopy(pull && byPlayer.WorldData.EntityControls.Sneak ? blockSel.Face : blockSel.Face.Opposite, distance);
        if (world.BlockAccessor.GetBlock(pos).IsReplacableBy(block))
        {
            world.BlockAccessor.SetBlock(0, blockSel.Position);
            world.BlockAccessor.SetBlock(block.BlockId, pos);
        }
        handling = EnumHandling.PreventDefault;
        return true;
    }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        distance = properties["distance"].AsInt(1);
        pull = properties["pull"].AsBool(false);
    }
}
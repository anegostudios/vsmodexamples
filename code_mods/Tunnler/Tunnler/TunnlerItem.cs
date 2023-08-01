using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Tunnler
{
    public class TunnlerItem : Item
    {

        public void destroyBlocks(IWorldAccessor world, BlockPos min, BlockPos max, IPlayer player)
        {
            BlockPos tempPos = new BlockPos();
            for (int x = min.X; x <= max.X; x++)
            {
                for (int y = min.Y; y <= max.Y; y++)
                {
                    for (int z = min.Z; z <= max.Z; z++)
                    {
                        tempPos.Set(x, y, z);
                        if (player.WorldData.CurrentGameMode == EnumGameMode.Creative)
                            world.BlockAccessor.SetBlock(0, tempPos);
                        else
                            world.BlockAccessor.BreakBlock(tempPos, player);
                    }
                }
            }
        }

        public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
        {
            if (base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel))
            {
                if (byEntity is EntityPlayer)
                {
                    IPlayer player = world.PlayerByUid((byEntity as EntityPlayer).PlayerUID);
                    switch (blockSel.Face.Axis)
                    {
                        case EnumAxis.X:
                            destroyBlocks(world, blockSel.Position.AddCopy(0, -1, -1), blockSel.Position.AddCopy(0, 1, 1), player);
                            break;
                        case EnumAxis.Y:
                            destroyBlocks(world, blockSel.Position.AddCopy(-1, 0, -1), blockSel.Position.AddCopy(1, 0, 1), player);
                            break;
                        case EnumAxis.Z:
                            destroyBlocks(world, blockSel.Position.AddCopy(-1, -1, 0), blockSel.Position.AddCopy(1, 1, 0), player);
                            break;
                    }
                }
                return true;
            }
            return false;
        }

    }

}
using System;
using Vintagestory.API;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Interfaces;

namespace Vintagestory.ModSamples
{
    /// <summary>
    /// Super basic example on how to set blocks in the game
    /// </summary>
    public class VillageGenerator : ModBase
    {
        ICoreServerAPI api;

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            
            this.api.RegisterCommand("house", "Place a house with an NPC inside (demo mod)", "", CmdGenHouse, Privilege.controlserver);
            this.api.RegisterCommand("block", "", "Places a block 2m in front of you", CmdBlock, Privilege.controlserver);
        }

        private void CmdBlock(IServerPlayer player, int groupId, CmdArgs args)
        {
            ushort blockID = api.WorldManager.GetBlockId("log-birch-ud");
            BlockPos pos = player.Entity.Pos.HorizontalAheadCopy(2).AsBlockPos;
            api.World.BlockAccessor.SetBlock(blockID, pos);
        }

        private void CmdGenHouse(IServerPlayer player, int groupId, CmdArgs args)
        {
            IBlockAccessor blockAccessor = api.WorldManager.GetBlockAccessorBulkUpdate(true, true);
            ushort blockID = api.WorldManager.GetBlockId("log-birch-ud");
            
            BlockPos pos = player.Entity.Pos.AsBlockPos;

            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dz = -3; dz <= 3; dz++)
                {
                    for (int dy = 0; dy <= 3; dy++)
                    {
                        if (Math.Abs(dx) != 3 && Math.Abs(dz) != 3 && dy < 3) continue; // Hollow
                        if (dx == -3 && dz == 0 && dy < 2) continue; // Door

                        blockAccessor.SetBlock(blockID, pos.AddCopy(dx, dy, dz));
                    }
                }
            }

            blockAccessor.Commit();

            api.AddNpc("Jeniffer", pos.UpCopy());
        }
    }
}
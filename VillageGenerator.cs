using System;
using Vintagestory.API;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Interfaces;

namespace Vintagestory.ServerMods
{
    public class VillageGenerator : ModBase
    {
        ICoreServerAPI api;

        public override void Start(ICoreServerAPI api)
        {
            this.api = api;
            this.api.RegisterCommand("house", "Place a house with an NPC inside (demo mod)", "", CmdGenHouse, Privilege.controlserver);
        }

        private void CmdGenHouse(int clientId, int groupId, string[] args)
        {
            IBlockAccesor blockAccessor = api.World.GetBlockAccessorBulkUpdate(true, true);
            ushort blockID = api.World.GetBlockId("log-birch-ud");
            
            BlockPos pos = api.Player.GetPosition(clientId).AsBlockPos;

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

            api.Player.AddNpc("Jeniffer", pos.UpCopy());
        }
    }
}
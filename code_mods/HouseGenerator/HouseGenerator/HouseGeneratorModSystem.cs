using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace HouseGenerator
{
    /// <summary>
    /// Super basic example on how to read/set blocks in the game
    /// </summary>
    public class HouseGenerator : ModSystem
    {
        ICoreServerAPI api;

        private void OnPlayerJoin(IServerPlayer byPlayer)
        {
            BlockPos plrpos = byPlayer.Entity.Pos.AsBlockPos;

            Block firebrickblock = api.World.GetBlock(new AssetLocation("claybricks-fire"));
            int blockId = firebrickblock.BlockId;
            api.World.BlockAccessor.SetBlock(blockId, plrpos.DownCopy());

            // Check a 3x3x3 area for logs
            int quantityLogs = 0;
            api.World.BlockAccessor.WalkBlocks(
                plrpos.AddCopy(-3, -3, -3),
                plrpos.AddCopy(3, 3, 3),
                (block, x,y,z) => quantityLogs += block.Code.Path.Contains("log") ? 1 : 0
            );

            byPlayer.SendMessage(GlobalConstants.GeneralChatGroup, "You have " + quantityLogs + " logs nearby you", EnumChatType.Notification);
        }



        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;

            api.Event.PlayerJoin += OnPlayerJoin;
            api.ChatCommands.Create("house")
            .RequiresPrivilege(Privilege.controlserver)
            .WithDescription("Places a house (sample mod)")
            .HandleWith(CmdGenHouse)
            .RequiresPlayer();

            api.ChatCommands.Create("block")
            .RequiresPrivilege(Privilege.controlserver)
            .WithDescription("Places a block 2m in front of you (sample mod)")
            .HandleWith(CmdBlock)
            .RequiresPlayer();
        }

        private TextCommandResult CmdBlock(TextCommandCallingArgs args)
        {
            int blockID = api.WorldManager.GetBlockId(new AssetLocation("log-placed-birch-ud"));
            BlockPos pos = args.Caller.Entity.Pos.HorizontalAheadCopy(2).AsBlockPos;
            api.World.BlockAccessor.SetBlock(blockID, pos);
            return TextCommandResult.Success();
        }

        private TextCommandResult CmdGenHouse(TextCommandCallingArgs args)
        {
            IBlockAccessor blockAccessor = api.World.GetBlockAccessorBulkUpdate(true, true);
            int blockID = api.WorldManager.GetBlockId(new AssetLocation("log-placed-oak-ud"));
            
            BlockPos pos = args.Caller.Entity.Pos.AsBlockPos;

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
            return TextCommandResult.Success();
        }
    }
}
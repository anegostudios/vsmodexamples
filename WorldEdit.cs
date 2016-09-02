using System;
using System.Collections.Generic;
using Vintagestory.API;

namespace Vintagestory.ServerMods
{
    public class ExampleModWorldEdit : ModBase
    {
        ICoreAPI api;
        BlockPos startMarker, endMarker;

        // Because I'm too lazy to type it over and over again ;-)
        int clientId;
        int groupId;


        public override void Start(ICoreAPI api)
        {
            this.api = api;

            api.Server.RegisterPrivilege("worldeditx", "Ability to use world edit tools");

            api.RegisterCommand("xwe", "World edit tools (example mod)", "[clear|ms|me|clearm|block|fillm]", CmdEdit, "worldeditx");
        }


        void Good(string message)
        {
            api.Player.SendMessage(clientId, groupId, message, EnumChatType.CommandSuccess);
        }
        void Bad(string message)
        {
            api.Player.SendMessage(clientId, groupId, message, EnumChatType.CommandError);
        }

        private void CmdEdit(int clientId, int groupId, string[] args)
        {
            this.clientId = clientId;
            this.groupId = groupId;

            BlockPos centerPos = api.Player.GetPosition(clientId).AsBlockPos;

            if (args.Length == 0)
            {
                Bad("Invalid arguments");
                return;
            }
            switch (args[0])
            {
                case "blocklineup":
                    BlockLineup(centerPos);
                    Good("Block lineup created");
                    break;
                case "ms":
                    startMarker = centerPos;
                    Good("Start position " + startMarker + " marked");
                    break;

                case "me":
                    endMarker = centerPos;
                    Good("End position " + endMarker + " marked");
                    break;

                case "fillm":
                    if (startMarker == null || endMarker == null)
                    {
                        Bad("Start marker or end marker not set");
                        return;
                    }

                    IPlayerInventoryManager plrInv = api.Player.GetPlayerInventoryManager(clientId);
                    IItemStack stack = plrInv.GetSelectedHotbarSlot().Itemstack;

                    if (stack == null || stack.ItemClass == EnumItemClass.Item)
                    {
                        Bad("Please put the desired block in your active hotbar slot");
                        return;
                    }

                    int filled = FillArea((ushort)stack.ItemId, startMarker, endMarker);

                    Good(filled + " marked blocks placed");

                    break;

                case "clearm":
                    {
                        if (startMarker == null || endMarker == null)
                        {
                            Bad("Start marker or end marker not set");
                            return;
                        }

                        int cleared = FillArea(0, startMarker, endMarker);
                        Good(cleared + " marked blocks cleared");
                    }
                    break;

                case "clear":
                    {
                        if (args.Length < 2)
                        {
                            Bad("Missing size param");
                            return;
                        }

                        int size = 0;
                        if (!int.TryParse(args[1], out size))
                        {
                            Bad("Invalide size param");
                            return;
                        }

                        int height = 20;
                        if (args.Length > 2)
                        {
                            int.TryParse(args[2], out height);
                        }


                        int cleared = FillArea(0, centerPos.OffsetCopy(-size, 0, -size), centerPos.OffsetCopy(size, height, size));

                        Good(cleared + " Blocks cleared");
                    }

                    break;

                default:
                    Bad("No such function " + args[0]);
                    break;
                        
            }
        }

        private void BlockLineup(BlockPos pos)
        {
            IBlockAccesor blockAccess = api.World.GetBlockAccessorBulkUpdate(true, true);
            Block[] blocks = api.World.GetBlockTypes();


            List<Block> existingBlocks = new List<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] == null || blocks[i].Code == null) continue;
                existingBlocks.Add(blocks[i]);
            }

            int width = (int)Math.Sqrt(existingBlocks.Count);

            FillArea(0, pos.OffsetCopy(0, 0, 0), pos.OffsetCopy(width + 1, 10, width + 1));

            for (int i = 0; i < existingBlocks.Count; i++)
            {
                if (existingBlocks[i] == null || existingBlocks[i].Code == null) continue;

                blockAccess.SetBlock(blocks[i].BlockId, pos.OffsetCopy(i / width, 0, i % width));
            }

            blockAccess.Commit();
        }

        private int FillArea(ushort blockId, BlockPos start, BlockPos end)
        {
            int updated = 0;

            IBlockAccesor blockAcccessor = api.World.GetBlockAccessorBulkUpdate(true, true);

            BlockPos startPos = new BlockPos(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Min(start.Z, end.Z));
            BlockPos finalPos = new BlockPos(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y), Math.Max(start.Z, end.Z));
            BlockPos curPos = startPos.Copy();

            int dx = finalPos.X - startPos.X;
            int dy = finalPos.Y - startPos.Y;
            int dz = finalPos.Z - startPos.Z;
            if (dx * dy * dz > 1000)
            {
                Good((blockId == 0 ? "Clearing" : "Placing") + " " + (dx * dy * dz) + " blocks...");
            }

            while (curPos.X < finalPos.X)
            {
                curPos.Y = startPos.Y;

                while (curPos.Y < finalPos.Y)
                {
                    curPos.Z = startPos.Z;
                    while (curPos.Z < finalPos.Z)
                    {
                        blockAcccessor.SetBlock(blockId, curPos);
                        curPos.Z++;
                        updated++;
                    }

                    curPos.Y++;
                }
                curPos.X++;
            }

            blockAcccessor.Commit();

            return updated;
        }
    }
}

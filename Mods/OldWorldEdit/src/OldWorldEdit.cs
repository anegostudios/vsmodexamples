using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace OldWorldEdit
{
    /// <summary>
    /// This is an old version of the worldedit mod that always ships with the game. Might be an interesting read for you in understanding how to go about in making world edit tools.
    /// </summary>
    public class OldWorldEdit : ModSystem
    {
        ICoreServerAPI api;
        BlockPos startMarker, endMarker;

        // Because I'm too lazy to type it over and over again ;-)
        IServerPlayer player;
        int groupId;

        string exportFolderPath;
        

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;

            exportFolderPath = api.GetOrCreateDataPath("WorldEdit");

            api.Permissions.RegisterPrivilege("worldedit", "Ability to use world edit tools");

            api.RegisterCommand("wo", "World edit tools (Old Version)", "[ms|me|mc|mex|cla|clm|fillm|blu]", CmdEdit, "worldedit");
        }


        void Good(string message)
        {
            player.SendMessage(groupId, message, EnumChatType.CommandSuccess);
        }

        void Bad(string message)
        {
            player.SendMessage(groupId, message, EnumChatType.CommandError);
        }

        private void CmdEdit(IServerPlayer player, int groupId, CmdArgs args)
        {
            this.player = player;
            this.groupId = groupId;

            BlockPos centerPos = player.Entity.Pos.AsBlockPos;

            if (args.Length == 0)
            {
                Bad("Invalid arguments");
                return;
            }
            switch (args[0])
            {
                case "mex":

                    if (startMarker == null || endMarker == null)
                    {
                        Bad("Please mark start and end position");
                        break;
                    }

                    if (args.Length < 2)
                    {
                        Bad("Please provide a filename");
                        break;
                    }

                    ExportArea(args[1], startMarker, endMarker);
                    break;

                case "imp":

                    if (startMarker == null)
                    {
                        Bad("Please mark a start position");
                        break;
                    }

                    if (args.Length < 2)
                    {
                        Bad("Please provide a filename");
                        break;
                    }

                    EnumOrigin origin = EnumOrigin.StartPos;

                    if (args.Length > 2)
                    {
                        try
                        {
                            origin = (EnumOrigin)Enum.Parse(typeof(EnumOrigin), args[2]);
                        }
                        catch (Exception)
                        {

                        }
                    }

                    ImportArea(args[1], startMarker, origin);

                    break;


                case "blu":
                    BlockLineup(centerPos);
                    Good("Block lineup created");
                    break;
                
                // Mark start
                case "ms":
                    startMarker = centerPos;
                    Good("Start position " + startMarker + " marked");
                    break;

                // Mark end
                case "me":
                    endMarker = centerPos;
                    Good("End position " + endMarker + " marked");
                    break;

                // Mark clear
                case "mc":
                    startMarker = null;
                    endMarker = null;
                    Good("Marked positions cleared");
                    break;

                // Fill marked
                case "fillm":
                    if (startMarker == null || endMarker == null)
                    {
                        Bad("Start marker or end marker not set");
                        return;
                    }

                    IPlayerInventoryManager plrInv = player.InventoryManager;
                    IItemStack stack = plrInv.ActiveHotbarSlot.Itemstack;

                    if (stack == null || stack.Class == EnumItemClass.Item)
                    {
                        Bad("Please put the desired block in your active hotbar slot");
                        return;
                    }

                    int filled = FillArea((ushort)stack.Id, startMarker, endMarker);

                    Good(filled + " marked blocks placed");

                    break;

                // Clear marked
                case "clm":
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

                // Clear area
                case "cla":
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


                        int cleared = FillArea(0, centerPos.AddCopy(-size, 0, -size), centerPos.AddCopy(size, height, size));

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
            IBlockAccessor blockAccess = api.WorldManager.GetBlockAccessorBulkUpdate(true, true);
            List<Block> blocks = api.World.Blocks;


            List<Block> existingBlocks = new List<Block>();
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == null || blocks[i].Code == null) continue;
                existingBlocks.Add(blocks[i]);
            }

            int width = (int)Math.Sqrt(existingBlocks.Count);

            FillArea(0, pos.AddCopy(0, 0, 0), pos.AddCopy(width + 1, 10, width + 1));

            for (int i = 0; i < existingBlocks.Count; i++)
            {
                if (existingBlocks[i] == null || existingBlocks[i].Code == null) continue;

                blockAccess.SetBlock(blocks[i].BlockId, pos.AddCopy(i / width, 0, i % width));
            }

            blockAccess.Commit();
        }

        private int FillArea(ushort blockId, BlockPos start, BlockPos end)
        {
            int updated = 0;

            IBlockAccessor blockAcccessor = api.WorldManager.GetBlockAccessorBulkUpdate(true, true);

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


        private void ImportArea(string filename, BlockPos startPos, EnumOrigin origin)
        {
            string infilepath = Path.Combine(exportFolderPath, filename);

            if (!File.Exists(infilepath) && File.Exists(infilepath + ".json"))
            {
                infilepath += ".json";
            }

            if (!File.Exists(infilepath))
            {
                Bad("Can't import " + filename + ", it does not exist");
                return;
            }

            BlockSchematic blockdata = null;

            try
            {
                using (TextReader textReader = new StreamReader(infilepath))
                {
                    blockdata = JsonConvert.DeserializeObject<BlockSchematic>(textReader.ReadToEnd());
                    textReader.Close();
                }
            }
            catch (IOException e)
            {
                Good("Failed loading " + filename + " : " + e.Message);
                return;
            }

            BlockPos originPos = startPos.Copy();

            if (origin == EnumOrigin.TopCenter)
            {
                originPos.X -= blockdata.SizeX / 2;
                originPos.Y -= blockdata.SizeY;
                originPos.Z -= blockdata.SizeZ / 2;
            }
            if (origin == EnumOrigin.BottomCenter)
            {
                originPos.X -= blockdata.SizeX / 2;
                originPos.Z -= blockdata.SizeZ / 2;
            }


            IBlockAccessor blockAcccessor = api.WorldManager.GetBlockAccessorBulkUpdate(true, true, false);

            blockdata.Place(blockAcccessor, api.World, originPos);

            blockAcccessor.Commit();
        }


        private void ExportArea(string filename, BlockPos start, BlockPos end)
        {
            int exported = 0;

            IBlockAccessor blockAcccessor = api.WorldManager.GetBlockAccessor(false, false, false);

            BlockPos startPos = new BlockPos(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Min(start.Z, end.Z));
            BlockPos finalPos = new BlockPos(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y), Math.Max(start.Z, end.Z));

            BlockSchematic blockdata = new BlockSchematic();

            for (int x = startPos.X; x < finalPos.X; x++)
            {
                for (int y = startPos.Y; y < finalPos.Y; y++)
                {
                    for (int z = startPos.Z; z < finalPos.Z; z++)
                    {
                        BlockPos pos = new BlockPos(x, y, z);
                        int blockid = blockAcccessor.GetBlockId(pos);
                        if (blockid == 0) continue;

                        blockdata.BlocksUnpacked[pos] = blockid;
                        exported++;
                    }
                }
            }

            blockdata.Pack(api.World, startPos);

            string outfilepath = Path.Combine(exportFolderPath, filename);

            if (!outfilepath.EndsWith(".json"))
            {
                outfilepath += ".json";
            }

            try
            {
                using (TextWriter textWriter = new StreamWriter(outfilepath))
                {
                    textWriter.Write(JsonConvert.SerializeObject(blockdata, Formatting.None));
                    textWriter.Close();
                }
            }
            catch (IOException e)
            {
                Good("Failed exporting: " + e.Message);
                return;
            }

            Good(exported + " blocks exported.");
        }
    }
}

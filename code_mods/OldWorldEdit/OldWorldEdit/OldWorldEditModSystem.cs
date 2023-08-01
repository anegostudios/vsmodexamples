using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
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

        string exportFolderPath;

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;

            exportFolderPath = api.GetOrCreateDataPath("WorldEdit");

            api.Permissions.RegisterPrivilege("worldedit", "Ability to use world edit tools");
            var parsers = api.ChatCommands.Parsers;
            api.ChatCommands.Create("wo")
            .WithDescription("World edit tools (Old Version)")
            .RequiresPrivilege("worldedit")
            .RequiresPlayer()
            .BeginSubCommand("ms")
                .WithDescription("Mark start")
                .HandleWith(args =>{
                    startMarker = args.Caller.Entity.Pos.AsBlockPos;
                    return TextCommandResult.Success("Start marked: "+ startMarker);
                })
            .EndSubCommand()

            .BeginSubCommand("imp")
                .WithDescription("Import schematic")
                .WithArgs(parsers.Word("filename"), parsers.OptionalInt("origin"))
                .HandleWith(args =>{
                    if (startMarker == null || endMarker == null)
                    {
                        return TextCommandResult.Error("Please mark start and end position");
                    }

                    if (args.Parsers[0].IsMissing)
                    {
                        return TextCommandResult.Error("Please provide a filename");
                    }

                    EnumOrigin origin = EnumOrigin.StartPos;

                    if (!args.Parsers[1].IsMissing)
                    {
                        try
                        {
                            origin = (EnumOrigin)Enum.Parse(typeof(EnumOrigin), (string)args.Parsers[1].GetValue());
                        }
                        catch (Exception)
                        {

                        }
                    }

                    return ImportArea((string)args.Parsers[0].GetValue(), startMarker, origin);
                })
            .EndSubCommand()

            .BeginSubCommand("me")
                .WithDescription("Mark end")
                .HandleWith(args =>{
                    endMarker = args.Caller.Entity.Pos.AsBlockPos;
                    return TextCommandResult.Success("Start marked: "+ endMarker);
                })
            .EndSubCommand()

            .BeginSubCommand("mc")
                .WithDescription("Mark clear")
                .HandleWith(args =>{
                    startMarker = null;
                    endMarker = null;
                    return TextCommandResult.Success("Marks cleared");
                })
            .EndSubCommand()

            .BeginSubCommand("mex")
                .WithDescription("Export schmatic")
                .WithArgs(parsers.Word("filename"))
                .HandleWith(args =>{
                    if (startMarker == null || endMarker == null)
                    {
                        return TextCommandResult.Error("Please mark start and end position");
                    }

                    if (args.Parsers[0].IsMissing)
                    {
                        return TextCommandResult.Error("Please provide a filename");
                    }

                    return ExportArea((string)args.Parsers[0].GetValue(), startMarker, endMarker);
                })
            .EndSubCommand()

            .BeginSubCommand("cla")
                .WithDescription("Clear area")
                .WithArgs(parsers.Int("size"), parsers.OptionalInt("height", 20))
                .HandleWith(args =>{
                    if (args.Parsers[0].IsMissing)
                    {
                        return TextCommandResult.Error("Missing size param");
                    }
                    var centerPos = args.Caller.Entity.Pos.AsBlockPos;

                    int size = (int)args.Parsers[0].GetValue();
                    int height = (int)args.Parsers[1].GetValue();
                    var player = args.Caller.Player as IServerPlayer;
                    int cleared = FillArea(0, centerPos.AddCopy(-size, 0, -size), centerPos.AddCopy(size, height, size), player);

                    return TextCommandResult.Success(cleared + " Blocks cleared");
                })
            .EndSubCommand()

            .BeginSubCommand("clm")
                .WithDescription("Clear marked")
                .HandleWith(args =>{
                    if (startMarker == null || endMarker == null)
                    {
                        return TextCommandResult.Error("Start marker or end marker not set");
                    }
                    var player = args.Caller.Player as IServerPlayer;
                    int cleared = FillArea(0, startMarker, endMarker, player);
                    return TextCommandResult.Success(cleared + " marked blocks cleared");
                })
            .EndSubCommand()

            .BeginSubCommand("fillm")
                .WithDescription("Fill marked")
                .HandleWith(args =>{
                    if (startMarker == null || endMarker == null)
                    {
                        return TextCommandResult.Error("Start marker or end marker not set");
                    }

                    IPlayerInventoryManager plrInv = args.Caller.Player.InventoryManager;
                    IItemStack stack = plrInv.ActiveHotbarSlot.Itemstack;

                    if (stack == null || stack.Class == EnumItemClass.Item)
                    {
                        return TextCommandResult.Error("Please put the desired block in your active hotbar slot");
                    }
                    var player = args.Caller.Player as IServerPlayer;
                    int filled = FillArea((ushort)stack.Id, startMarker, endMarker, player);


                    return TextCommandResult.Success(filled + " marked blocks placed");
                })
            .EndSubCommand()

            .BeginSubCommand("blu")
                .WithDescription("Block lineup")
                .HandleWith(args =>{
                    BlockLineup(args.Caller.Entity.Pos.AsBlockPos, args.Caller.Player as IServerPlayer);
                    return TextCommandResult.Success("Block lineup created");
                })
            .EndSubCommand();

        }

        private void BlockLineup(BlockPos pos, IServerPlayer player)
        {
            IBlockAccessor blockAccess = api.World.GetBlockAccessorBulkUpdate(true, true);
            IList<Block> blocks = api.World.Blocks;


            List<Block> existingBlocks = new List<Block>();
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == null || blocks[i].Code == null) continue;
                existingBlocks.Add(blocks[i]);
            }

            int width = (int)Math.Sqrt(existingBlocks.Count);

            FillArea(0, pos.AddCopy(0, 0, 0), pos.AddCopy(width + 1, 10, width + 1), player);

            for (int i = 0; i < existingBlocks.Count; i++)
            {
                if (existingBlocks[i] == null || existingBlocks[i].Code == null) continue;

                blockAccess.SetBlock(blocks[i].BlockId, pos.AddCopy(i / width, 0, i % width));
            }

            blockAccess.Commit();
        }

        private int FillArea(ushort blockId, BlockPos start, BlockPos end, IServerPlayer player)
        {
            int updated = 0;

            IBlockAccessor blockAcccessor = api.World.GetBlockAccessorBulkUpdate(true, true);

            BlockPos startPos = new BlockPos(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Min(start.Z, end.Z));
            BlockPos finalPos = new BlockPos(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y), Math.Max(start.Z, end.Z));
            BlockPos curPos = startPos.Copy();

            int dx = finalPos.X - startPos.X;
            int dy = finalPos.Y - startPos.Y;
            int dz = finalPos.Z - startPos.Z;

            if (dx * dy * dz > 1000)
            {
                //TODO
                player.SendMessage(GlobalConstants.GeneralChatGroup, (blockId == 0 ? "Clearing" : "Placing") + " " + (dx * dy * dz) + " blocks...", EnumChatType.CommandSuccess);
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

        private TextCommandResult ImportArea(string filename, BlockPos startPos, EnumOrigin origin)
        {
            string infilepath = Path.Combine(exportFolderPath, filename);

            if (!File.Exists(infilepath) && File.Exists(infilepath + ".json"))
            {
                infilepath += ".json";
            }

            if (!File.Exists(infilepath))
            {
                
                return TextCommandResult.Error("Can't import " + filename + ", it does not exist");
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
                return TextCommandResult.Error("Failed loading " + filename + " : " + e.Message);
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


            IBlockAccessor blockAcccessor = api.World.GetBlockAccessorBulkUpdate(true, true, false);

            blockdata.Place(blockAcccessor, api.World, originPos);

            blockAcccessor.Commit();
            return TextCommandResult.Success("Imported");
        }

        private TextCommandResult ExportArea(string filename, BlockPos start, BlockPos end)
        {
            int exported = 0;

            IBlockAccessor blockAcccessor = api.World.GetBlockAccessor(false, false, false);

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
                return TextCommandResult.Success("Failed exporting: " + e.Message);
            }

            return TextCommandResult.Success(exported + " blocks exported.");
        }
    }
}

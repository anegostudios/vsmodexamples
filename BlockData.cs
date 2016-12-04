using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Interfaces;

namespace Vintagestory.ModSamples
{
    /// <summary>
    /// This class can be used to export/import block data from/to json files. You have to call Pack before saving and Unpack before placing. 
    /// Call AddArea to add a simple cubicle area of blocks
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BlockData
    {
        [JsonProperty]
        public int SizeX;
        [JsonProperty]
        public int SizeY;
        [JsonProperty]
        public int SizeZ;
        [JsonProperty]
        public Dictionary<int, string> BlockCodes = new Dictionary<int, string>();
        [JsonProperty]
        public List<uint> Indices = new List<uint>();
        [JsonProperty]
        public List<int> BlockIds = new List<int>();


        public int Angle;
        public bool Flipped;


        public Dictionary<BlockPos, ushort> BlocksUnpacked = new Dictionary<BlockPos, ushort>();



        public void AddArea(IBlockAccessor blockAccess, BlockPos start, BlockPos end)
        {
            BlockPos startPos = new BlockPos(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Min(start.Z, end.Z));
            BlockPos finalPos = new BlockPos(Math.Max(start.X, end.X), Math.Max(start.Y, end.Y), Math.Max(start.Z, end.Z));

            for (int x = startPos.X; x < finalPos.X; x++)
            {
                for (int y = startPos.Y; y < finalPos.Y; y++)
                {
                    for (int z = startPos.Z; z < finalPos.Z; z++)
                    {
                        BlockPos pos = new BlockPos(x, y, z);
                        ushort blockid = blockAccess.GetBlockId(pos);
                        if (blockid == 0) continue;

                        BlocksUnpacked[pos] = blockid;
                    }
                }
            }
        }


        public bool Pack(IBlockAccessor blockAccessor, BlockPos startPos)
        {
            BlockCodes.Clear();
            Indices.Clear();
            BlockIds.Clear();
            SizeX = 0;
            SizeY = 0;
            SizeZ = 0;

            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;

            foreach (var val in BlocksUnpacked)
            {
                minX = Math.Min(minX, val.Key.X);
                minY = Math.Min(minY, val.Key.Y);
                minZ = Math.Min(minZ, val.Key.Z);

                // Store relative position and the block id
                int dx = val.Key.X - startPos.X;
                int dy = val.Key.Y - startPos.Y;
                int dz = val.Key.Z - startPos.Z;

                if (dx >= 1024 || dy >= 1024 || dz >= 1024)
                {
                    Console.WriteLine("Export format does not support areas larger than 1024 blocks in any direction");
                    return false;
                }
            }

            foreach (var val in BlocksUnpacked)
            {
                if (val.Value == 0) continue;

                // Store a block mapping
                string blockCode = blockAccessor.GetBlockType(val.Value).Code;
                BlockCodes[val.Value] = blockCode;

                // Store relative position and the block id
                int dx = val.Key.X - minX;
                int dy = val.Key.Y - minY;
                int dz = val.Key.Z - minZ;

                SizeX = Math.Max(dx, SizeX);
                SizeY = Math.Max(dy, SizeY);
                SizeZ = Math.Max(dz, SizeZ);

                Indices.Add((uint)((dy << 20) | (dz << 10) | dx));
                BlockIds.Add(val.Value);
            }

            return true;
        }

        public void Unpack(IBlockAccessor blockAccessor, BlockPos startPos)
        {
            BlocksUnpacked.Clear();

            for (int i = 0; i < Indices.Count; i++)
            {
                uint index = Indices[i];
                int storedBlockid = BlockIds[i];

                int dx = (int)(index & 0x1ff);
                int dy = (int)((index >> 20) & 0x1ff);
                int dz = (int)((index >> 10) & 0x1ff);

                if (Flipped)
                {
                    dy = SizeY - dy;
                }

                string blockCode = BlockCodes[storedBlockid];
                ushort blockId;

                if (Angle != 0)
                {
                    string newCode = blockAccessor.GetBlockType(blockCode).GetRotatedBlockCode(Angle);
                    blockId = blockAccessor.GetBlockType(newCode).BlockId;
                } else
                {
                    blockId = blockAccessor.GetBlockType(blockCode).BlockId;
                }

                dx -= SizeX / 2;
                dz -= SizeX / 2;

                BlockPos pos = new BlockPos(dx, dy, dz);

                // 90 deg:
                // xNew = -yOld
                // yNew = xOld

                // 180 deg:
                // xNew = -xOld
                // yNew = -yOld

                // 270 deg:
                // xNew = yOld
                // yNew = -xOld

                switch (Angle)
                {
                    case 90:
                        pos.Set(-dz, dy, dx);
                        break;
                    case 180:
                        pos.Set(-dx, dy, -dz);
                        break;
                    case 270:
                        pos.Set(dz, dy, -dx);
                        break;
                }

                pos.X += SizeX / 2;
                pos.Z += SizeZ / 2;

                BlocksUnpacked[pos.Add(startPos)] = blockId;
            }
        }


        public int Place(IBlockAccessor blockAccessor, EnumReplaceMode mode = EnumReplaceMode.Replaceable)
        {
            int placed = 0;
            if (mode == EnumReplaceMode.ReplaceAll) {
                foreach (var val in BlocksUnpacked)
                {
                    blockAccessor.SetBlock(val.Value, val.Key);
                    placed++;
                }
            } else
            {
                foreach (var val in BlocksUnpacked)
                {
                    if (mode == EnumReplaceMode.ReplaceAir)
                    {
                        if (blockAccessor.GetBlockId(val.Key) != 0) continue;
                    } else
                    {
                        if (blockAccessor.GetBlockType(val.Key).Replaceable <= blockAccessor.GetBlockType(val.Value).Replaceable) continue;
                    }
                    

                    blockAccessor.SetBlock(val.Value, val.Key);
                    placed++;
                }

            }

            return placed;
        }


        public BlockPos[] GetJustPositions(BlockPos origin)
        {
            BlockPos[] positions = new BlockPos[Indices.Count];

            for (int i = 0; i < Indices.Count; i++)
            {
                uint index = Indices[i];
                int storedBlockid = BlockIds[i];

                int dx = (int)(index & 0x1ff);
                int dy = (int)((index >> 20) & 0x1ff);
                int dz = (int)((index >> 10) & 0x1ff);

                dx -= SizeX / 2;
                dz -= SizeZ / 2;

                if (Flipped)
                {
                    dy = SizeY - dy;
                }

                BlockPos pos = new BlockPos(dx, dy, dz);
                switch (Angle)
                {
                    case 90:
                        pos.Set(-dz, dy, dx);
                        break;
                    case 180:
                        pos.Set(-dx, dy, -dz);
                        break;
                    case 270:
                        pos.Set(dz, dy, -dx);
                        break;
                }

                pos.X += SizeX / 2;
                pos.Z += SizeZ / 2;

                

                positions[i] = pos.Add(origin);
            }

            return positions;
        }


        public static BlockData Load(string infilepath)
        {
            if (!File.Exists(infilepath) && File.Exists(infilepath + ".json"))
            {
                infilepath += ".json";
            }

            if (!File.Exists(infilepath))
            {
                throw new Exception("Can't import " + infilepath + ", it does not exist");
            }

            BlockData blockdata = null;

            try
            {
                using (TextReader textReader = new StreamReader(infilepath))
                {
                    blockdata = JsonConvert.DeserializeObject<BlockData>(textReader.ReadToEnd());
                    textReader.Close();
                }
            }
            catch (IOException e)
            {
                throw new Exception("Failed loading " + infilepath + " : " + e.Message);
            }

            return blockdata;
        }


        public string Save(string outfilepath)
        {
            if (!outfilepath.EndsWith(".json"))
            {
                outfilepath += ".json";
            }

            try
            {
                using (TextWriter textWriter = new StreamWriter(outfilepath))
                {
                    textWriter.Write(JsonConvert.SerializeObject(this, Formatting.None));
                    textWriter.Close();
                }
            }
            catch (IOException e)
            {
                return "Failed exporting: " + e.Message;
            }

            return null;
        }


        public BlockPos GetStartPos(BlockPos pos, EnumOrigin origin)
        {
            BlockPos originPos = pos.Copy();

            if (origin == EnumOrigin.TopCenter)
            {
                originPos.X -= SizeX / 2;
                originPos.Y -= SizeY;
                originPos.Z -= SizeZ / 2;
            }
            if (origin == EnumOrigin.BottomCenter)
            {
                originPos.X -= SizeX / 2;
                originPos.Z -= SizeZ / 2;
            }

            return originPos;
        }


        internal void Flip()
        {
            Flipped = !Flipped;
        }

    }



    public enum EnumReplaceMode
    {
        Replaceable,
        ReplaceAll,
        ReplaceAir,
    }

    public enum EnumOrigin
    {
        StartPos = 0,
        BottomCenter = 1,
        TopCenter = 2
    }


}

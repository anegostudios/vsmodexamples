using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Interfaces;

namespace Vintagestory.ServerMods
{
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


        public Dictionary<BlockPos, ushort> BlocksUnpacked = new Dictionary<BlockPos, ushort>();

        public bool Pack(IBlockAccesor blockAccessor, BlockPos startPos)
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

        public void Unpack(IBlockAccesor blockAccessor, BlockPos startPos)
        {
            BlocksUnpacked.Clear();

            for (int i = 0; i < Indices.Count; i++)
            {
                uint index = Indices[i];
                int storedBlockid = BlockIds[i];

                int dx = (int)(index & 0x1ff);
                int dy = (int)((index >> 20) & 0x1ff);
                int dz = (int)((index >> 10) & 0x1ff);

                string blockCode = BlockCodes[storedBlockid];
                ushort blockId = blockAccessor.GetBlockType(blockCode).BlockId;

                BlocksUnpacked[startPos.AddCopy(dx, dy, dz)] = blockId;
            }
        }


        public int Place(IBlockAccesor blockAccessor)
        {
            int placed = 0;
            foreach (var val in BlocksUnpacked)
            {
                blockAccessor.SetBlock(val.Value, val.Key);
                placed++;
            }

            return placed;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace VSTreasureChest
{
    /// <summary>
    /// Mod that places chests filled with random items at the base of trees. Also supports a /treasure command for 
    /// placing a chest in front of the player.
    /// </summary>
    public class TreasureChestMod : ModSystem
    {
        //The minimum number of items to place in the chest
        private const int MinItems = 3;

        //The maximum number of items to place in the chest
        private const int MaxItems = 10;

        //The max number of chests to generate per chunk
        private const int MaxChestsPerChunk = 1;

        //The probability of a chest spawning in a chunk
        private const float ChestSpawnProbability = 0.80f;

        //The main interface we will use for interacting with Vintage Story
        private ICoreServerAPI _api;

        //Size of chunks. Chunks are cubes so this is the size of the cube.
        private int _chunkSize;

        //Stores tree types that will be used for detecting trees for placing our chests
        private ISet<string> _treeTypes;

        //Used for accessing blocks during chunk generation
        private IBlockAccessor _chunkGenBlockAccessor;

        //Used for accessing blocks after chunk generation
        private IBlockAccessor _worldBlockAccessor;

        /// <summary>
        /// This is our starting point. This method will be called by the server.
        /// </summary>
        public override void StartServerSide(ICoreServerAPI api)
        {
            _api = api;
            _worldBlockAccessor = api.World.BlockAccessor;
            _chunkSize = _worldBlockAccessor.ChunkSize;
            _treeTypes = new HashSet<string>();
            LoadTreeTypes(_treeTypes);

            api.ChatCommands.Create("treasure")
            .WithDescription("Place a treasure chest with random items")
            .RequiresPrivilege(Privilege.controlserver)
            .RequiresPlayer()
            .HandleWith(PlaceTreasureChestInFrontOfPlayer);

            //Registers a delegate to be called so we can get a reference to the chunk gen block accessor
            _api.Event.GetWorldgenBlockAccessor(OnWorldGenBlockAccessor);

            //Registers a delegate to be called when a chunk column is generating in the Vegetation phase of generation
            _api.Event.ChunkColumnGeneration(OnChunkColumnGeneration, EnumWorldGenPass.PreDone, "standard");
        }

        /// <summary>
        /// Our mod only needs to be loaded by the server
        /// </summary>
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        ///<summary>
        /// Loads tree types from worldproperties/block/wood.json. Used for detecting trees for chest placement.
        /// </summary>
        private void LoadTreeTypes(ISet<string> treeTypes)
        {
            var treeTypesFromFile = _api.Assets.TryGet("worldproperties/block/wood.json").ToObject<StandardWorldProperty>();
            foreach (var variant in treeTypesFromFile.Variants)
            {
                treeTypes.Add($"log-grown-{variant.Code.Path}-ud");
            }
        }

        /// <summary>
        /// Stores the chunk gen thread's IBlockAccessor for use when generating chests during chunk gen. This callback
        /// is necessary because chunk loading happens in a separate thread and it's important to use this block accessor
        /// when placing chests during chunk gen.
        /// </summary>
        private void OnWorldGenBlockAccessor(IChunkProviderThread chunkProvider)
        {
            _chunkGenBlockAccessor = chunkProvider.GetBlockAccessor(true);
        }

        /// <summary>
        /// Called when a number of chunks have been generated. For each chunk we first determine if we should place a chest
        /// and if we should we then loop through each block to find a tree. When one is found we place the block at the base
        /// of the tree. At most one chest will be placed per chunk.
        /// </summary>
        /// 
        private void OnChunkColumnGeneration(IChunkColumnGenerateRequest request)
        {
            //IServerChunk[] chunks, int chunkX, int chunkZ, ITreeAttribute chunkgenparams
            var chestsPlacedCount = 0;
            for (var i = 0; i < request.Chunks.Length; i++)
            {
                if (ShouldPlaceChest())
                {
                    var blockPos = new BlockPos();
                    for (var x = 0; x < _chunkSize; x++)
                    {
                        for (var z = 0; z < _chunkSize; z++)
                        {
                            for (var y = 0; y < _worldBlockAccessor.MapSizeY; y++)
                            {
                                if (chestsPlacedCount < MaxChestsPerChunk)
                                {
                                    blockPos.X = request.ChunkX * _chunkSize + x;
                                    blockPos.Y = y;
                                    blockPos.Z = request.ChunkZ * _chunkSize + z;

                                    var chestLocation = TryGetChestLocation(blockPos);
                                    if (chestLocation != null)
                                    {
                                        var chestWasPlaced = PlaceTreasureChest(_chunkGenBlockAccessor, chestLocation);
                                        if (chestWasPlaced)
                                        {
                                            chestsPlacedCount++;
                                        }
                                    }
                                }
                                else //Max chests have been placed for this chunk
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the location to place the chest if the given world coordinates is a tree, null if it's not a tree.
        /// </summary>
        private BlockPos TryGetChestLocation(BlockPos pos)
        {
            var block = _chunkGenBlockAccessor.GetBlock(pos);
            if (IsTreeLog(block))
            {
                for (var posY = pos.Y; posY >= 0; posY--)
                {
                    while (pos.Y-- > 0)
                    {
                        var underBlock = _chunkGenBlockAccessor.GetBlock(pos);
                        if (IsTreeLog(underBlock))
                        {
                            continue;
                        }

                        foreach (var facing in BlockFacing.HORIZONTALS)
                        {
                            var adjacentPos = pos.AddCopy(facing).Up();
                            if (_chunkGenBlockAccessor.GetBlock(adjacentPos).Id == 0)
                            {
                                return adjacentPos;
                            }
                        }
                    }
                }
            }

            return null;
        }

        private bool IsTreeLog(Block block)
        {
            return _treeTypes.Contains(block.Code.Path);
        }

        /// <summary>
        /// Delegate for /treasure command. Places a treasure chest 2 blocks in front of the player
        /// </summary>
        private TextCommandResult PlaceTreasureChestInFrontOfPlayer(TextCommandCallingArgs args)
        {
            PlaceTreasureChest(_api.World.BlockAccessor, args.Caller.Entity.Pos.HorizontalAheadCopy(2).AsBlockPos);
            return TextCommandResult.Success();
        }

        /// <summary>
        /// Places a chest filled with random items at the given world coordinates using the given IBlockAccessor
        /// </summary>
        private bool PlaceTreasureChest(IBlockAccessor blockAccessor, BlockPos pos)
        {
            var blockID = _api.WorldManager.GetBlockId(new AssetLocation("chest-south"));
            var chest = _api.World.BlockAccessor.GetBlock(blockID);

            if (chest.TryPlaceBlockForWorldGen(blockAccessor, pos, BlockFacing.UP, null))
            {
                var block = blockAccessor.GetBlock(pos);
                if (block.EntityClass != chest.EntityClass)
                {
                    return false;
                }

                var blockEntity = blockAccessor.GetBlockEntity(pos);
                if(blockEntity != null){
                    blockEntity.Initialize(_api);
                    if (blockEntity is IBlockEntityContainer chestEntity)
                    {
                        AddItemStacks(chestEntity, MakeItemStacks());
                        Debug.WriteLine("Placed treasure chest at " + pos, new object[] { });
                        return true;
                    }
                }
            }

            Debug.WriteLine("FAILED TO PLACE TREASURE CHEST AT " + pos, new object[] { });
            return false;
        }

        private bool ShouldPlaceChest()
        {
            var randomNumber = _api.World.Rand.Next(0, 100);
            return randomNumber > 0 && randomNumber <= ChestSpawnProbability * 100;
        }

        /// <summary>
        /// Makes a list of random ItemStacks to be placed inside our chest
        /// </summary>
        private IEnumerable<ItemStack> MakeItemStacks()
        {
            var shuffleBag = MakeShuffleBag();
            var itemStacks = new Dictionary<string, ItemStack>();
            var grabCount = _api.World.Rand.Next(MinItems, MaxItems);
            for (var i = 0; i < grabCount; i++)
            {
                var nextItem = shuffleBag.Next();
                var item = _api.World.GetItem(new AssetLocation(nextItem));
                if (itemStacks.ContainsKey(nextItem))
                {
                    itemStacks[nextItem].StackSize++;
                }
                else
                {
                    itemStacks.Add(nextItem, new ItemStack(item));
                }
            }

            return itemStacks.Values;
        }

        ///<summary>
        ///Adds the given list of ItemStacks to the first slots in the given chest.
        ///</summary>
        private void AddItemStacks(IBlockEntityContainer chest, IEnumerable<ItemStack> itemStacks)
        {
            var slotNumber = 0;
            foreach (var itemStack in itemStacks)
            {
                slotNumber = Math.Min(slotNumber, chest.Inventory.Count - 1);
                var slot = chest.Inventory[slotNumber];
                slot.Itemstack = itemStack;
                slotNumber++;
            }
        }

        /// <summary>
        /// Creates our ShuffleBag to pick from when generating items for the chest
        /// </summary>
        private ShuffleBag<string> MakeShuffleBag()
        {
            var shuffleBag = new ShuffleBag<string>(100, _api.World.Rand);
            shuffleBag.Add("ingot-iron", 10);
            shuffleBag.Add("ingot-bismuth", 5);
            shuffleBag.Add("ingot-silver", 5);
            shuffleBag.Add("ingot-zinc", 5);
            shuffleBag.Add("ingot-titanium", 5);
            shuffleBag.Add("ingot-platinum", 5);
            shuffleBag.Add("ingot-chromium", 5);
            shuffleBag.Add("ingot-tin", 5);
            shuffleBag.Add("ingot-lead", 5);
            shuffleBag.Add("ingot-gold", 5);
            return shuffleBag;
        }
    }
}
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Server;
using Vintagestory.ServerMods;
using SkiaSharp;
using System.Collections.Generic;
using Vintagestory.Common;

/**

    Notes on the world generation.

    The world generation is made in several passes. The passes are defined in the enum EnumWorldGenPass.
    
    Within a pass, the generation is done in layers are ordered based on the index given by the function ExecuteOrder(). 
    The loop in StartServerSide deletes all currently registered world gen path (therefore with an index lower than this class ExecuteOrder).

    The vanilla passes are the following:
        -      0      GenTerra : creates the vanilla landscape heights.
        -      0.1    GenRockStrata : layers the GenTerra height in different materials.
        -      0.12   GenDungeons : generates the underground dungeons.
        -      0.2    GenDeposits : generates the ore deposits.
        -      0.3    GenStructures : generates the world structures.
        -      0.4    GenBlockLayers : generates ice, grass...
        -      0.5    GenPonds : generates lakes...
        -      0.5    GenVegetationAndPatches : generates forests and plants.

    The source code of the vanilla world generation can be found in the VS survival source code:
    https://github.com/anegostudios/vssurvivalmod.git/Systems/WorldGen/Standard

    This mode replaces the layers GenTerra and GenRockStrata to create a map based on a .png image. 
    The generation is purposously split in several steps for better code readability:
        -      0      AlpineTerrain : Generates the granite main layer, and a layer of dirt on top of it (replaces GenTerra).
        -      1      AlpineStrata : Adds a layer of another stone below the surface using the vanilla noise generator (replaces GenRockStrata).
        -      2      AlpineFloor : parametrise the data that are used later to generate plants and the block cover.
        -      3      AlpineFloorCorrection : Correcting some of the vanilla world gen features that don't match the expected result (dirt layer too thick, dirt or snow on cliff side).
        -      4      AlpineRiver : Remove the plants from the rivers/lakes.

*/
namespace AlpineStoryMod
{
    public class AlpineStoryModSystem : ModStdWorldGen
    {
        ICoreServerAPI api;
        SKBitmap height_map;
        internal float data_width_per_pixel;
        internal int min_height_custom = 90; 
        AlpineTerrain alpineTerrain;
        AlpineStrata alpineStrata;
        AlpineFloor alpineFloor;
        AlpineFloorCorrection alpineFloorCorrection;
        AlpineRiver alpineRiver;
        BiomeGrid biomeGrid;
        int[] regionMap;
        int[] lakeMap;
        UtilTool uTool;
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        //  The ExecuteOrder here is important as it has to be after all worldgen objects we want to delete (see StartServerSide function).
        public override double ExecuteOrder()
        {
            return 0.2;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;

            //  Removing all generator that was registered so far
            foreach (int enumvalue in Enum.GetValues(typeof(EnumWorldGenPass)))
            {
                if (enumvalue < ((ServerMain)api.World).ModEventManager.WorldgenHandlers["standard"].OnChunkColumnGen.Length)
                {
                    if (enumvalue != (int)EnumWorldGenPass.NeighbourSunLightFlood && enumvalue != (int)EnumWorldGenPass.PreDone)
                    {
                        var handlers = ((ServerMain)api.World).ModEventManager.WorldgenHandlers["standard"].OnChunkColumnGen[enumvalue] ;
                        List<int> toRemove = new List<int>();

                        if(handlers != null)
                        {
                            //  Condition on which object type we want to remove
                            for (int i = 0; i < handlers.Count; i++){
                                var type = handlers[i].Method.DeclaringType;
                                if (type == typeof(GenTerra) || 
                                    type == typeof(GenRockStrataNew) || 
                                    type == typeof(GenTerraPostProcess)){
                                    toRemove.Add(i);
                                }
                            }
                            for (int i = toRemove.Count - 1; i >= 0 ; i--){
                                handlers.RemoveAt(toRemove[i]) ;
                            }
                        }
                    }
                }
            }

            //  In this mod, the X - Z coordinates are scaled based on the map Y size
            data_width_per_pixel = api.WorldManager.MapSizeY / 256;

            //  Change this boolean to True to generate a climate (temperature - rain) mapping, for debug/information purpose.
            bool generateBiomeGrid = false ;

            //  We give a 2048 - 2048 offset of the map to not start on the borders
            uTool = new UtilTool(api, 2048, 2048);

            if(generateBiomeGrid){
                biomeGrid = new BiomeGrid(api, height_map, data_width_per_pixel, min_height_custom);
                api.Event.ChunkColumnGeneration(biomeGrid.OnChunkColumnGen, EnumWorldGenPass.Terrain, "standard");
            }
            else{
                //  Reading the height map that will be provided to all world generation passes
                
                IAsset asset = this.api.Assets.Get(new AssetLocation("alpinestory:worldgen/processed.png"));
                BitmapExternal bmpt = new BitmapExternal(asset.Data, asset.Data.Length, api.Logger);
                height_map = bmpt.bmp;

                if(height_map == null){
                    uTool.print("Current directory : "+System.IO.Directory.GetCurrentDirectory());
                    uTool.print("Files in current dir : "+System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory()).ToString());
                    throw new Exception("Height map not found");
                }

                api.Logger.Notification("Image loaded : "+height_map.Width.ToString()+", "+height_map.Height.ToString());  

                chunksize = api.WorldManager.ChunkSize;

                //  The region maps correspond to macro maps of the world, (one pixel per chunk)
                //      -   regionMap is used to set climates based on the local altitude
                //      -   lakeMap is used to forbid forest in lakes
                regionMap = uTool.build_region_map(height_map, api.WorldManager.ChunkSize, data_width_per_pixel, min_height_custom, api.WorldManager.MapSizeY, 0);
                lakeMap = uTool.build_region_map(height_map, api.WorldManager.ChunkSize, data_width_per_pixel, min_height_custom, api.WorldManager.MapSizeY, 1);

                //  Int random generator used as criterion to spawn halite
                Random rand = new Random();

                //  Creating an instance of each generation function
                alpineTerrain = new AlpineTerrain(api, height_map, data_width_per_pixel, min_height_custom, uTool);
                alpineStrata = new AlpineStrata(api, height_map, data_width_per_pixel, min_height_custom, uTool);
                alpineFloor = new AlpineFloor(api, height_map, data_width_per_pixel, min_height_custom, regionMap, lakeMap, uTool);
                alpineFloorCorrection = new AlpineFloorCorrection(api, height_map, data_width_per_pixel, min_height_custom, regionMap, rand, uTool);
                alpineRiver = new AlpineRiver(api, height_map, data_width_per_pixel, min_height_custom, regionMap, uTool);

                //  Registering the generation function in the Terrain pass. It is not necessary to have them stored in different files.
                api.Event.ChunkColumnGeneration(alpineTerrain.OnChunkColumnGen, EnumWorldGenPass.Terrain, "standard");
                api.Event.ChunkColumnGeneration(alpineStrata.OnChunkColumnGen, EnumWorldGenPass.Terrain, "standard");
                api.Event.ChunkColumnGeneration(alpineFloor.OnChunkColumnGen, EnumWorldGenPass.Terrain, "standard");
                api.Event.ChunkColumnGeneration(alpineFloorCorrection.OnChunkColumnGen, EnumWorldGenPass.TerrainFeatures, "standard");
                api.Event.ChunkColumnGeneration(alpineRiver.OnChunkColumnGen, EnumWorldGenPass.NeighbourSunLightFlood, "standard");
            }

            //  Don't you dare removing that line, it would silently break some pass of the vanilla world gen.
            api.Event.InitWorldGenerator(initWorldGen, "standard");
        }
        
        public void initWorldGen()
        {
            LoadGlobalConfig(api);
        }
    }
}
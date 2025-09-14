using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;
using Vintagestory.ServerMods.NoObf;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

public class BiomeGrid: ModStdWorldGen
{
    ICoreServerAPI api;
    int maxThreads;
    internal SKBitmap height_map;
    internal float data_width_per_pixel;        
    internal int max_height_custom;    
    internal int bare_land_height_custom;
    internal int min_height_custom; 
    internal UtilTool uTool;
    ColumnResult[] columnResults;
    public BiomeGrid(){}
    public BiomeGrid(ICoreServerAPI api, SKBitmap height_map, float data_width_per_pixel, int min_height_custom)
    {
        LoadGlobalConfig(api);
        
        this.api = api;
        this.height_map = height_map;

        //  The ColumnResult object will contain the data of the chunks to generate
        columnResults = new ColumnResult[chunksize * chunksize];
        for (int i = 0; i < chunksize * chunksize; i++) columnResults[i].ColumnBlockSolidities = new BitArray(api.WorldManager.MapSizeY);
        
        
        this.api = api;
        this.height_map = height_map;
        
        maxThreads = Math.Min(Environment.ProcessorCount, api.Server.Config.HostedMode ? 4 : 10);
        max_height_custom = api.WorldManager.MapSizeY;
        bare_land_height_custom = (int) (max_height_custom*0.9);

        this.data_width_per_pixel = data_width_per_pixel;
        this.min_height_custom = min_height_custom;

        uTool = new UtilTool();

        }
    public void OnChunkColumnGen(IChunkColumnGenerateRequest request)
    {   
        generate(request.Chunks, request.ChunkX, request.ChunkZ, request.RequiresChunkBorderSmoothing);
    }

    public override double ExecuteOrder()
    {
        return 1.5;
    }
    private void generate(IServerChunk[] chunks, int chunkX, int chunkZ, bool requiresChunkBorderSmoothing)
    {
        int chunksize = this.chunksize;

        int rockID = api.World.GetBlock(new AssetLocation("rock-granite")).Id ;

        // // Store heightmap in the map chunk that can be used for ingame weather processing.
        ushort[] rainheightmap = chunks[0].MapChunk.RainHeightMap;
        ushort[] terrainheightmap = chunks[0].MapChunk.WorldGenTerrainHeightMap;
        
        //  For each X - Z coordinate of the chunk, storing the data in the column result. Multithreaded for faster process
        Parallel.For(0, chunksize * chunksize, new ParallelOptions() { MaxDegreeOfParallelism = maxThreads }, chunkIndex2d => {

            int current_thread = Thread.CurrentThread.ManagedThreadId;

            int lX = chunkIndex2d % chunksize;
            int lZ = chunkIndex2d / chunksize;
            int worldX = chunkX * chunksize + lX;
            int worldZ = chunkZ * chunksize + lZ;
            
            BitArray columnBlockSolidities = columnResults[chunkIndex2d].ColumnBlockSolidities;

            for (int posY = 1; posY < max_height_custom - 1; posY++)
            {
                //  The block solidity tells if the block will not be empty after the first pass.
                columnBlockSolidities[posY] = posY < 150;
            }
        });

        chunks[0].Data.SetBlockBulk(0, chunksize, chunksize, GlobalConfig.mantleBlockId);

        /**
            Setting the blocks data here.

            The content of the chunks is stored in chunks[verticalChunkId].Data, which is an int array of size chunksize^3.

            The Id to provide can be given by the following function, "rock-granite" being the name of a block for example. 
                api.World.GetBlock(new AssetLocation("rock-granite")).Id ;

        */
        for (int posY = 1; posY < max_height_custom - 1; posY++)
        {
            for (int lZ = 0; lZ < chunksize; lZ++)
            {
                int worldZ = chunkZ * chunksize + lZ;
                for (int lX = 0; lX < chunksize; lX++)
                {
                    int worldX = chunkX * chunksize + lX;

                    int mapIndex = uTool.ChunkIndex2d(lX, lZ, chunksize);

                    ColumnResult columnResult = columnResults[mapIndex];
                    bool isSolid = columnResult.ColumnBlockSolidities[posY];

                    if (isSolid)
                    {
                        terrainheightmap[mapIndex] = (ushort)posY;
                        rainheightmap[mapIndex] = (ushort)posY;

                        uTool.setBlockId(lX, posY, lZ, chunksize, chunks, rockID);
                    }
                }
            }
        }

        ushort ymax = 0;
        for (int i = 0; i < rainheightmap.Length; i++)
        {
            ymax = Math.Max(ymax, rainheightmap[i]);
        }

        chunks[0].MapChunk.YMax = ymax;

        //     Holds a forest density map, from 0 to 255
        IntDataMap2D forestMap = chunks[0].MapChunk.MapRegion.ForestMap; 
        for(int i = 0; i < forestMap.Data.Length; i++){
            forestMap.Data[i] = 150;
        }

        //     Holds temperature and rain fall.
        //     16-23 bits = Red = temperature - 0 : frozen, 255 : all hail the cactus. (Height dependance adds to this parameter)
        //     8-15 bits = Green = rain
        //     0-7 bits = Blue = unused 
        IntDataMap2D climateMap = chunks[0].MapChunk.MapRegion.ClimateMap;

        int grid_size = 16;
        float factor = 255/(grid_size-1);

        for(int i = 0; i < climateMap.Data.Length; i++){
            int fakeChunkX = chunkX - chunkX%16 - climateMap.BottomRightPadding + i%climateMap.Size;
            int fakeChunkZ = chunkZ - chunkZ%16 - climateMap.BottomRightPadding + i/climateMap.Size;
            
            int rain = fakeChunkX%grid_size;
            int temp = fakeChunkZ%grid_size;
            
            climateMap.Data[i] = (int)(0 + Math.Min(255, (int)(factor*rain))*Math.Pow(2, 8) +  Math.Min(255, (int)(factor*temp))*Math.Pow(2, 16)) ;
        }
        
        // //     Holds a beach density map
        IntDataMap2D beachMap = chunks[0].MapChunk.MapRegion.BeachMap;
        beachMap.Data = new int[beachMap.Size*beachMap.Size];

        // //  Bushes density map, from 0 to 255
        IntDataMap2D shrubMap = chunks[0].MapChunk.MapRegion.ShrubMap;

        for(int i = 0; i < shrubMap.Data.Length; i++){
            shrubMap.Data[i] = 255;
        }
    }
}
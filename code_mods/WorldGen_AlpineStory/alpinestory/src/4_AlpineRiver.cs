using System;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;
using SkiaSharp;

public class AlpineRiver: ModStdWorldGen
{
    ICoreServerAPI api;
    internal SKBitmap height_map;
    internal float data_width_per_pixel;        
    internal int max_height_custom;    
    internal int min_height_custom; 
    internal UtilTool uTool;
    internal int[] regionMap;
    public AlpineRiver(){}
    public AlpineRiver(ICoreServerAPI api, SKBitmap height_map, float data_width_per_pixel, int min_height_custom, int[] regionMap, UtilTool uTool)
    {
        LoadGlobalConfig(api);
        
        this.api = api;
        this.height_map = height_map;
        
        this.min_height_custom = min_height_custom;
        this.max_height_custom = api.WorldManager.MapSizeY;

        this.data_width_per_pixel = data_width_per_pixel;
        this.regionMap = regionMap;

        this.uTool = uTool;
    }
    public void OnChunkColumnGen(IChunkColumnGenerateRequest request)
    {   
        generate(request.Chunks, request.ChunkX, request.ChunkZ, request.RequiresChunkBorderSmoothing);
    }

    public override double ExecuteOrder()
    {
        return 0.05;
    }
    private void generate(IServerChunk[] chunks, int chunkX, int chunkZ, bool requiresChunkBorderSmoothing)
    { 
        //  We reiterate the river and lake making here, to remove the plants generated underwater by the vanilla worldgen.
        int muddyGravelID = api.World.GetBlock(new AssetLocation("muddygravel")).Id ;        
        int waterID = api.World.GetBlock(new AssetLocation("water-still-7")).Id ;

        //  Clean river beds
        cleanRiverBed(chunks, chunkX, chunkZ, waterID, muddyGravelID);

        //  Clean river beds
        uTool.makeLakes(chunks, chunkX, chunkZ, chunksize, waterID, muddyGravelID, min_height_custom, max_height_custom, data_width_per_pixel, height_map);

    }
    public void cleanRiverBed(IServerChunk[] chunks, int chunkX, int chunkZ, int waterID, int gravelID){
        float hasRiver;
        int altitude;
        int localRiverHeight;
        
        for (int lZ = 0; lZ < chunksize*chunksize; lZ++){
            int worldX = chunkX * chunksize + lZ%chunksize+ uTool.offsetX;
            int worldZ = chunkZ * chunksize + lZ/chunksize + uTool.offsetZ;

            hasRiver = uTool.LerpPosHeight(worldX, worldZ, 2, data_width_per_pixel, height_map);

            if(hasRiver > 0.1){
                altitude = (int) (min_height_custom + (max_height_custom - min_height_custom) * uTool.LerpPosHeight(worldX, worldZ, 0, data_width_per_pixel, height_map));
                
                //  Checking if we are not removing a tree
                if(uTool.getBlockId(lZ%chunksize, altitude-3, lZ/chunksize, chunksize, chunks) == 0 ||
                    (uTool.getBlockId(lZ%chunksize, altitude-3, lZ/chunksize, chunksize, chunks) != 
                        uTool.getBlockId(lZ%chunksize, altitude+1, lZ/chunksize, chunksize, chunks))){

                    localRiverHeight = uTool.getRiverHeight(worldX, worldZ, min_height_custom, max_height_custom, data_width_per_pixel, height_map);

                    for(int posY = altitude-2; posY < localRiverHeight; posY++){
                        uTool.SetBlockAir(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks);
                        uTool.setBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks, waterID, fluid:true);
                    }

                    if (altitude-2 == localRiverHeight){
                        uTool.SetBlockAir(lZ%chunksize, localRiverHeight, lZ/chunksize, chunksize, chunks);
                        uTool.setBlockId(lZ%chunksize, localRiverHeight, lZ/chunksize, chunksize, chunks, waterID, fluid:true);
                    }

                    for(int posY = localRiverHeight; posY < localRiverHeight+3; posY++){
                        uTool.SetBlockAir(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks);
                    }
                }

                uTool.setBlockId(lZ%chunksize, altitude-4, lZ/chunksize, chunksize, chunks, gravelID);
                uTool.setBlockId(lZ%chunksize, altitude-3, lZ/chunksize, chunksize, chunks, gravelID);
            }
        }
    }    
}
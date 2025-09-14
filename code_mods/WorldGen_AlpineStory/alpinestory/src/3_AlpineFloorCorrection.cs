using System;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;
using SkiaSharp;
using System.Linq;

public class AlpineFloorCorrection: ModStdWorldGen
{
    ICoreServerAPI api;
    int maxThreads;
    internal SKBitmap height_map;
    internal float data_width_per_pixel;        
    internal int max_height_custom;    
    internal int bare_land_height_custom;
    internal int min_height_custom; 
    internal UtilTool uTool;
    internal int[] regionMap;
    internal Random rand;
    public AlpineFloorCorrection(){}
    public AlpineFloorCorrection(ICoreServerAPI api, SKBitmap height_map, float data_width_per_pixel, int min_height_custom, int[] regionMap, Random rand, UtilTool uTool)
    {
        LoadGlobalConfig(api);
        
        this.api = api;
        this.height_map = height_map;
        
        maxThreads = Math.Min(Environment.ProcessorCount, api.Server.Config.HostedMode ? 4 : 10);
        max_height_custom = api.WorldManager.MapSizeY;
        bare_land_height_custom = (int) (max_height_custom*0.9);

        this.data_width_per_pixel = data_width_per_pixel;
        this.min_height_custom = min_height_custom;
        this.regionMap = regionMap;
        this.rand = rand;

        this.uTool = uTool;
    }
    public void OnChunkColumnGen(IChunkColumnGenerateRequest request)
    {   
        generate(request.Chunks, request.ChunkX, request.ChunkZ, request.RequiresChunkBorderSmoothing);
    }

    public override double ExecuteOrder()
    {
        return 0.55;
    }
    private void generate(IServerChunk[] chunks, int chunkX, int chunkZ, bool requiresChunkBorderSmoothing)
    { 
        int[] soil_array = new int[]{api.World.GetBlock(new AssetLocation("soil-medium-normal")).Id,
                                        api.World.GetBlock(new AssetLocation("soil-medium-sparse")).Id , 
                                        api.World.GetBlock(new AssetLocation("soil-medium-verysparse")).Id , 
                                        api.World.GetBlock(new AssetLocation("soil-medium-none")).Id, 
                                        api.World.GetBlock(new AssetLocation("soil-low-normal")).Id, 
                                        api.World.GetBlock(new AssetLocation("soil-low-sparse")).Id, 
                                        api.World.GetBlock(new AssetLocation("soil-low-verysparse")).Id, 
                                        api.World.GetBlock(new AssetLocation("soil-low-none")).Id} ;

        int rockID = api.World.GetBlock(new AssetLocation("rock-granite")).Id ;
        int gravelID = api.World.GetBlock(new AssetLocation("gravel-granite")).Id ;
        int muddyGravelID = api.World.GetBlock(new AssetLocation("muddygravel")).Id ;
        
        int snowID = api.World.GetBlock(new AssetLocation("snowblock")).Id ;
        int glacierID = api.World.GetBlock(new AssetLocation("glacierice")).Id ;
        int waterID = api.World.GetBlock(new AssetLocation("water-still-7")).Id ;

        int haliteID = api.World.GetBlock(new AssetLocation("rock-halite")).Id;

        //  Builds the chunk height map, and reduces the dirt layer thickness on the go to let the rocks appear on cliffs
        int[] list_max_height = countHeightMap(chunks, rockID, snowID, glacierID, soil_array);

        int[] grass_map = uTool.analyse_chunk(list_max_height, chunkX, chunkZ, chunksize, min_height_custom, max_height_custom, data_width_per_pixel, height_map, 1);
        int[] remove_snow_map = uTool.analyse_chunk(list_max_height, chunkX, chunkZ, chunksize, min_height_custom, max_height_custom, data_width_per_pixel, height_map, 2);

        //  Replacing dirt by grass if the landscape is too steep
        clearSteepGrass(chunks, grass_map, list_max_height, gravelID);

        //  Replacing now and ice by rock if the landscape is too steep
        clearSteepSnow(chunks, remove_snow_map, gravelID);

        //  Removing glaciers that are not overlayed by snow
        clearGlacier(chunks, rockID, glacierID, snowID);

        //  Make river beds
        makeRiverBed(chunks, chunkX, chunkZ, waterID, muddyGravelID);

        //  Make lakes
        uTool.makeLakes(chunks, chunkX, chunkZ, chunksize, waterID, muddyGravelID, min_height_custom, max_height_custom, data_width_per_pixel, height_map);

        //  Spawn Halite every 30 chunk
        if (rand.Next(30) == 1){
            spawnHalite(chunks, haliteID);
        }
    }
    public void spawnHalite(IServerChunk[] chunks, int haliteID){
        int posY;
        int firstVoidFound;
        int lX, lZ;
        int haliteRadius = 4;   // halite column radius

        //  Here, we loop on coluns to find a cave below the ground
        //  If we find one, we create a halite column starting at that height + 5 to the bottom of the map
        for (int blockColumn = 0; blockColumn < chunksize*chunksize; blockColumn++){
            lX = blockColumn%chunksize;
            lZ = blockColumn/chunksize;
            if (lX > haliteRadius && lZ > haliteRadius && lX < chunksize - haliteRadius && lZ < chunksize - haliteRadius){
                posY = 1;

                while(uTool.getBlockId(blockColumn%chunksize, posY, blockColumn/chunksize, chunksize, chunks) !=0) posY++;
                firstVoidFound = posY;

                while(posY < 0.9*max_height_custom && uTool.getBlockId(blockColumn%chunksize, posY, blockColumn/chunksize, chunksize, chunks) ==0) posY++;
                if(posY < 0.9*max_height_custom){
                    for(int i=-haliteRadius; i<haliteRadius+1; i++){
                        for(int j=-haliteRadius; i<haliteRadius+1; i++){
                            if (Math.Abs(i) + Math.Abs(j) <= haliteRadius){
                                for(int k = 1; k < firstVoidFound + 5 ; k++){
                                    uTool.setBlockId(lX+i, k, lZ+j, chunksize, chunks, haliteID);
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }            
    }
    public void makeRiverBed(IServerChunk[] chunks, int chunkX, int chunkZ, int waterID, int gravelID){
        float hasRiver;
        int altitude;
        int localRiverHeight;
        
        //  Filling the river with water, and replaces the ground with two layers of muddy gravel
        //  The water height is taken minimal in its surrounding to not have weird behavior at the surface
        for (int lZ = 0; lZ < chunksize*chunksize; lZ++){
            int worldX = chunkX * chunksize + lZ%chunksize+ uTool.offsetX;
            int worldZ = chunkZ * chunksize + lZ/chunksize + uTool.offsetZ;

            hasRiver = uTool.LerpPosHeight(worldX, worldZ, 2, data_width_per_pixel, height_map);

            if(hasRiver > 0.1){
                altitude = (int) (min_height_custom + (max_height_custom - min_height_custom) * uTool.LerpPosHeight(worldX, worldZ, 0, data_width_per_pixel, height_map));
                
                localRiverHeight = uTool.getRiverHeight(worldX, worldZ, min_height_custom, max_height_custom, data_width_per_pixel, height_map);

                for(int posY = altitude-2; posY < localRiverHeight; posY++){
                    uTool.setBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks, waterID, fluid:true);
                }

                if (altitude-2 == localRiverHeight){
                    uTool.setBlockId(lZ%chunksize, localRiverHeight, lZ/chunksize, chunksize, chunks, waterID, fluid:true);
                }

                uTool.setBlockId(lZ%chunksize, altitude-4, lZ/chunksize, chunksize, chunks, gravelID);
                uTool.setBlockId(lZ%chunksize, altitude-3, lZ/chunksize, chunksize, chunks, gravelID);
            }
        }
    }    
    public void clearGlacier(IServerChunk[] chunks, int rockID, int glacierID, int snowID){
        int posY;

        //  Removes the glacier blocks that doesn't have snow block on it (looks more natural)
        for (int lZ = 0; lZ < chunksize*chunksize; lZ++){
            posY = max_height_custom - 1;

            while(uTool.getBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks) != snowID && 
                    posY > 0.6*max_height_custom) {
                    if(uTool.getBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks) == glacierID){
                        uTool.setBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks, rockID);
                    }
                    posY --;
            }
        }
    }    
    public void clearSteepSnow(IServerChunk[] chunks, int[] remove_snow_map, int rockID){
        int posY;

        //  Replacing snow by rock if the landscape is too steep
        for (int lZ = 0; lZ < chunksize*chunksize; lZ++){
            if (remove_snow_map[lZ] == 1){
                posY = max_height_custom - 1;

                while(uTool.getBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks) == 0) posY --;

                while(uTool.getBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks) != rockID 
                        && uTool.getBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks) != 0 && 
                        posY > 2) {
                    uTool.setBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks, rockID);
                    posY --;
                }
            }
        }
    }
    public void clearSteepGrass(IServerChunk[] chunks, int[] grass_map, int[] list_max_height, int gravelID){
        for (int lZ = 0; lZ < chunksize*chunksize; lZ++){
            if (grass_map[lZ] == 1){
                //  Replacing dirt by gravel if the landscape is too steep
                uTool.setBlockId(lZ%chunksize, list_max_height[lZ], lZ/chunksize, chunksize, chunks, gravelID);

                if(list_max_height[lZ] < max_height_custom - 3){
                    if (uTool.getBlockId(lZ%chunksize, list_max_height[lZ] + 1, lZ/chunksize, chunksize, chunks) != 0 
                            && uTool.getBlockId(lZ%chunksize, list_max_height[lZ] + 3, lZ/chunksize, chunksize, chunks) == 0 ){
                        uTool.setBlockId(lZ%chunksize, list_max_height[lZ] + 1, lZ/chunksize, chunksize, chunks, 0);
                        uTool.setBlockId(lZ%chunksize, list_max_height[lZ] + 2, lZ/chunksize, chunksize, chunks, 0);
                    }
                }
            }
        }
    }
    public int[] countHeightMap(IServerChunk[] chunks, int rockID, int snowID, int glacierID, int[] soil_array){
        int posY;
        int currentBlock;
        int mapIndex;
        int[] list_max_height = new int[chunksize*chunksize];

        for (int lZ = 0; lZ < chunksize; lZ++)
        {
            for (int lX = 0; lX < chunksize; lX++)
            {
                mapIndex = uTool.ChunkIndex2d(lX, lZ, chunksize);
                posY = max_height_custom - 1;

                while(uTool.getBlockId(lX, posY, lZ, chunksize, chunks) == 0) posY --;

                currentBlock = uTool.getBlockId(lX, posY, lZ, chunksize, chunks);

                while(currentBlock != rockID && !soil_array.Contains(currentBlock) && currentBlock != 0 && posY > 2) {
                    posY --;
                    currentBlock = uTool.getBlockId(lX, posY, lZ, chunksize, chunks);
                    if(currentBlock == snowID || currentBlock == glacierID){
                        list_max_height[mapIndex] = posY;
                    }
                }

                list_max_height[mapIndex] = Math.Max(posY, list_max_height[mapIndex]);

                //  Reducing the dirt layer thickness
                if(soil_array.Contains(currentBlock)){
                    posY -- ;

                    currentBlock = uTool.getBlockId(lX, posY, lZ, chunksize, chunks);
                    while (currentBlock != rockID  && currentBlock != 0 && posY > 2){
                        uTool.setBlockId(lX, posY, lZ, chunksize, chunks, rockID);
                        posY --;
                        currentBlock = uTool.getBlockId(lX, posY, lZ, chunksize, chunks);
                    }
                }
            }
        }
        return list_max_height;
    }
}
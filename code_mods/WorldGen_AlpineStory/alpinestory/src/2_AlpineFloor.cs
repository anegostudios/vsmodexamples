using System;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;
using SkiaSharp;

/*
    Once the rocks landscape is generated, the vanilla world generation will replace the top layers of rock with dirt/sand/gravel,
    and place animals based on regional maps.

    The different heights maps are defined in this object:
        chunks[0].MapChunk.MapRegion

    We can notably find:
        -   ForestMap : Defines the forest density
        -   ClimateMap : Defines the climate (rain and temperature)
        -   ShrubMap : Defines the shurbs/foliages density
    
    The region maps do not give the climate in the given chunk only but on the chunk in its surrounding.
    The origin of the region maps is set on the chunk coordinates (chunkX, chunkZ) modulo 16 (16 chunks in a region).

    Here is an example of what is expected in the climate map of the chunk (18, 25). the "o" are values to provide, 
    the "X" illustrates the position of the considered chunk. Borders are also to give to ensure the continuity of the 
    maps in the world.

       14  15      16  17  18  19  20  21  22  23  24  25  26  27  28  29  30  31      32  33
    14  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    15  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
        -------------------------------------------------------------------------------------
    16  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    17  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    18  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    19  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    20  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    21  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    22  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    23  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    24  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    25  o   o   |   o   o   X   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    26  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    27  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    28  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    29  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    30  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    31  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
       -------------------------------------------------------------------------------------
    32  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o
    33  o   o   |   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   o   |   o   o

    This map is stored in a 1D array using the same (X, Z) -> 1D mapping as used before.

    However, it appears the origin of the map (low X and Z) is not at 0, 0 but at (length - 1, length - 1),
    so the coordinates have to be flipped if the maps are set manually.

*/
public class AlpineFloor: ModStdWorldGen
{
    ICoreServerAPI api;
    internal SKBitmap height_map;
    internal float data_width_per_pixel;        
    internal int max_height_custom;    
    internal int bare_land_height_custom;
    internal int min_height_custom; 
    internal UtilTool uTool;
    int regionToChunkRatio;
    internal int[] regionMap, lakeMap;
    public AlpineFloor(){}
    public AlpineFloor(ICoreServerAPI api, SKBitmap height_map, float data_width_per_pixel, int min_height_custom, int[] regionMap, int[] lakeMap, UtilTool uTool)
    {
        LoadGlobalConfig(api);
        
        this.api = api;
        this.height_map = height_map;
        regionToChunkRatio = height_map.Width/chunksize;
        
        max_height_custom = api.WorldManager.MapSizeY;

        //  We decide that above 90% of the world height, there shouldn't be any tree/bush anymore
        bare_land_height_custom = (int) (max_height_custom*0.9);

        this.data_width_per_pixel = data_width_per_pixel;
        this.min_height_custom = min_height_custom;
        this.regionMap = regionMap;
        this.lakeMap = lakeMap;

        this.uTool = uTool;
    }
    public void OnChunkColumnGen(IChunkColumnGenerateRequest request)
    {   
        generate(request.Chunks, request.ChunkX, request.ChunkZ, request.RequiresChunkBorderSmoothing);
    }

    public override double ExecuteOrder()
    {
        return 0.15;
    }
    private void generate(IServerChunk[] chunks, int chunkX, int chunkZ, bool requiresChunkBorderSmoothing)
    {
        //  Setting the global forestation to zero to better control the forest density
        ITreeAttribute worldConfig = api.WorldManager.SaveGame.WorldConfiguration;
        worldConfig.SetString("globalForestation", "0.");

        //  This value tells how many chunks are in a "region"
        int globalRegionSize = api.WorldManager.RegionSize / chunksize;

        //  Offsetting the chunk by the same offset as defined in AlpineStoryModModSystem
        int fakeChunkX = chunkX + uTool.offsetX/chunksize;
        int fakeChunkZ = chunkZ + uTool.offsetZ/chunksize;

        //     Holds a forest density map, from 0 to 255
        IntDataMap2D forestMap = chunks[0].MapChunk.MapRegion.ForestMap; 

        //  build_mini_region_map builds a local region map as understood by the MapRegion tools.
        int[] forest_height_map = uTool.build_mini_region_map(forestMap, fakeChunkX, fakeChunkZ, regionToChunkRatio, regionMap, globalRegionSize, 1);
        int[] lake_height_map = uTool.build_mini_region_map(forestMap, fakeChunkX, fakeChunkZ, regionToChunkRatio, lakeMap, globalRegionSize, 1);
        
        //  If we are in a lake : no forest
        //  The forest takes the vanilla generated value, if the altitude is not too high
        for(int i = 0; i < chunks[0].MapChunk.MapRegion.ForestMap.Data.Length; i++){
            if (lake_height_map[i] == min_height_custom){
                forestMap.Data[i] = Math.Clamp(forestMap.Data[i]-1, 0, getForestFromHeight(forest_height_map[i])) ;
            }
            else{
                forestMap.Data[i] = 0;
            }
        }

        //     Holds temperature and rain fall.
        //     16-23 bits = Red = temperature - 0 : frozen, 255 : all hail the cactus. (Height dependance strongly adds to this parameter)
        //     8-15 bits = Green = rain
        //     0-7 bits = Blue = unused 
        IntDataMap2D climateMap = chunks[0].MapChunk.MapRegion.ClimateMap;
        climateMap.Data = uTool.build_mini_region_map(climateMap, fakeChunkX, fakeChunkZ, regionToChunkRatio, regionMap, globalRegionSize, 1);

        for(int i = 0; i < climateMap.Data.Length; i++){
            climateMap.Data[i] = (int)(0 + getRainFromHeight(climateMap.Data[i])*Math.Pow(2, 8) +  getTemperatureFromHeight(climateMap.Data[i])*Math.Pow(2, 16)) ;
        }
        
        //     Holds a beach density map
        //     No beach here, so all array set to 0
        IntDataMap2D beachMap = chunks[0].MapChunk.MapRegion.BeachMap;
        beachMap.Data = new int[beachMap.Size*beachMap.Size];

        //     Bushes density map, from 0 to 255
        //     The bushes density decreases with height
        IntDataMap2D shrubMap = chunks[0].MapChunk.MapRegion.ShrubMap;
        shrubMap.Data = uTool.build_mini_region_map(shrubMap, fakeChunkX, fakeChunkZ, regionToChunkRatio, regionMap, globalRegionSize, 0.5);

        for(int i = 0; i < shrubMap.Data.Length; i++){
            shrubMap.Data[i] = (int)getShrubFromHeight(shrubMap.Data[i]) ;
        }
    }
    private float getRelativeHeight(int height){
        return (float)(height - min_height_custom) / (float)(bare_land_height_custom - min_height_custom);
    }
    public int getRainFromHeight(int height){
        /*
            Returns 20000 at min_height_custom, linear increase with height
            desert below 90
        */   
        int min_value = 70;
        int max_value = 200;
        return (int)(min_value + (max_value - min_value) * getRelativeHeight(height));
    }
    
    public int getTemperatureFromHeight(int height){
        /*
            150 at min height
            80 at max height
        */
        int min_value = 80;
        int max_value = 150;
        return (int)(150 - (max_value - min_value) * getRelativeHeight(height));
    }    
    public int getShrubFromHeight(int height){
        if (height > bare_land_height_custom) return 0;

        return (int)(80 *(1 - getRelativeHeight(height)));
    }
    public int getForestFromHeight(int height){
        /*
            no forest above 60% of the bare_land_height_custom
            linear increase below this height
        */
        float relative_height = getRelativeHeight(height); 

        if (relative_height > 0.6){
            return 0;
        }
        else{
            return (int)(relative_height/0.6*255);
        }
    }
}
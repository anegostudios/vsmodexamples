using SkiaSharp;
using System;
using System.Collections;
using System.Linq;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

public class UtilTool
{
    internal ICoreServerAPI api;
    internal int offsetX;
    internal int offsetZ;
    public UtilTool(){}
    public UtilTool(ICoreServerAPI api, int offsetX, int offsetZ){
        this.api = api;
        this.offsetX = offsetX;
        this.offsetZ = offsetZ;
    }
    public int ChunkIndex3d(int x, int y, int z, int chunksize)
    {
        /*
            Vanilla function that gives the 1D index of a (x, y, z) coordinate in a chunk
        */
        return (y * chunksize + z) * chunksize + x;
    }
    public int ChunkIndex2d(int x, int z, int chunksize)
    {
        /*
            Vanilla function that gives the 1D index of a (x, z) coordinate in a chunk column
        */
        return z * chunksize + x;
    }
    public void setBlockId(int lX, int lY, int lZ, int chunksize, IServerChunk[] chunks, int blockId, bool fluid=false){
        /*
            Sets the block at (lX, lY, lZ) coordinate in a chunk column to the given value.
            If fluid is at true, the value is also assigned to the fluid data layer.
        */
        chunks[lY/chunksize].Data[ChunkIndex3d(lX, lY%chunksize, lZ, chunksize)] = blockId;

        if(fluid){
            chunks[lY/chunksize].Data[ChunkIndex3d(lX, lY%chunksize, lZ, chunksize)] = blockId;
            chunks[lY/chunksize].Data.SetFluid(ChunkIndex3d(lX, lY%chunksize, lZ, chunksize), blockId);
        }
    }
    public int getFluid(int lX, int lY, int lZ, int chunksize, IServerChunk[] chunks){
        /*
            Returns the fluid block ID at (lX, lY, lZ) coordinate in a chunk column.
        */
        return chunks[lY/chunksize].Data.GetFluid(ChunkIndex3d(lX, lY%chunksize, lZ, chunksize));
    }
    public int getBlockId(int lX, int lY, int lZ, int chunksize, IServerChunk[] chunks){
        /*
            Returns the block ID at (lX, lY, lZ) coordinate in a chunk column.
        */
        return chunks[lY/chunksize].Data[ChunkIndex3d(lX, lY%chunksize, lZ, chunksize)];
    }
    public void SetBlockAir(int lX, int lY, int lZ, int chunksize, IServerChunk[] chunks){
        /*
            Sets the block at (lX, lY, lZ) coordinate in a chunk column to an air block.
        */
        chunks[lY/chunksize].Data.SetBlockAir(ChunkIndex3d(lX, lY%chunksize, lZ, chunksize));
    }
    int mod(int x, int m) {
        /*
            Modulo function that necessarilly returns a value in [0, m[.
        */
        return (x%m + m)%m;
    }
    public int getOutBoundX(int x, int mapsize){
        /*
            Makes the symetry + modulo of the height map to fake an infinite world.
        */
        return (int)(Math.Abs(mapsize - 0.5 - mod(x, 2*mapsize))-0.5);
    }
    public float LerpPosHeight(int worldX, int worldZ, int color, float data_width_per_pixel, SKBitmap height_map){
        /*
            Lerp the Z value at the (worldX, worldZ) coordinates on the height map, on the given color chanel.
        */
        float current_x = (float)(worldX)/data_width_per_pixel ;
        float current_z = (float)(worldZ)/data_width_per_pixel ;

        float h0_0 = 0, h1_0 = 0, h0_1 = 0, h1_1 = 0;

         //     ICI TODO Cest a chier z0 a 4000, des trous a 2000 en worldX
        int x0 = getOutBoundX((int)current_x, height_map.Width);
        int z0 = getOutBoundX((int)current_z, height_map.Height);
        
        int x1 = getOutBoundX((int)current_x+1, height_map.Width);
        int z1 = getOutBoundX((int)current_z+1, height_map.Height);

        if(color==0){
            h0_0 = height_map.GetPixel(x0, z0).Red;
            h1_0 = height_map.GetPixel(x1, z0).Red;

            h0_1 = height_map.GetPixel(x0, z1).Red;
            h1_1 = height_map.GetPixel(x1, z1).Red;
        }
        else if (color==1){
            h0_0 = height_map.GetPixel(x0, z0).Green;
            h1_0 = height_map.GetPixel(x1, z0).Green;

            h0_1 = height_map.GetPixel(x0, z1).Green;
            h1_1 = height_map.GetPixel(x1, z1).Green;

        }
        else if (color==2){
            h0_0 = height_map.GetPixel(x0, z0).Blue;
            h1_0 = height_map.GetPixel(x1, z0).Blue;

            h0_1 = height_map.GetPixel(x0, z1).Blue;
            h1_1 = height_map.GetPixel(x1, z1).Blue;
        }

        float x = current_x%1;
        float z = current_z%1;

        return  ((1-x)*(1-z)*h0_0 + x*(1-z)*h1_0 + (1-x)*z*h0_1 + x*z*h1_1)/255;
    }

    public int[] analyse_chunk(int[] list_max_height, int chunkX, int chunkZ, int chunksize, int min_height_custom, int max_height_custom, float data_width_per_pixel, SKBitmap height_map, int criterion){
        /*
            Detects specific features on the height map, features description below.
        */
        int[] to_increase = new int[chunksize*chunksize];

        for (int lZ = 0; lZ < chunksize; lZ++)
        {
            for(int lX = 0; lX < chunksize; lX++){
                int[] neighbours = new int[4];

                if ((lX - 1 >= 0) && (lZ - 1 >= 0)){
                    neighbours[0] = list_max_height[ChunkIndex2d(lX-1, lZ-1, chunksize)];
                }
                
                if ((lX - 1 >= 0) && (lZ + 1 < chunksize)){
                    neighbours[1] = list_max_height[ChunkIndex2d(lX-1, lZ+1, chunksize)];
                }

                if ((lX + 1 < chunksize) && (lZ + 1 < chunksize)){
                    neighbours[2] = list_max_height[ChunkIndex2d(lX+1, lZ+1, chunksize)];
                }

                if ((lX + 1 < chunksize) && (lZ - 1 >= 0)){
                    neighbours[3] = list_max_height[ChunkIndex2d(lX+1, lZ-1, chunksize)];
                }

                for(int i=0; i<4; i++){
                    if(neighbours[i] == 0) neighbours[i] = list_max_height[ChunkIndex2d(lX, lZ, chunksize)];
                }

                //  Finds 2 blocks high steps
                if (criterion == 0){
                    if(neighbours.Max() > list_max_height[ChunkIndex2d(lX, lZ, chunksize)] + 1){
                        to_increase[ChunkIndex2d(lX, lZ, chunksize)] = 1;
                    }
                }
                //  Finds steep clifs
                else if (criterion == 1){
                    if(neighbours.Max() - neighbours.Min() > 3){
                        to_increase[ChunkIndex2d(lX, lZ, chunksize)] = 1;
                    }
                }
                //  Finds steep cliffs without its edges
                else if (criterion == 2){
                    if(Math.Max(neighbours.Max() - list_max_height[ChunkIndex2d(lX, lZ, chunksize)], list_max_height[ChunkIndex2d(lX, lZ, chunksize)] - neighbours.Min()) > 2
                    && Math.Min(neighbours.Max() - list_max_height[ChunkIndex2d(lX, lZ, chunksize)], list_max_height[ChunkIndex2d(lX, lZ, chunksize)] - neighbours.Min()) > 1 ){
                        to_increase[ChunkIndex2d(lX, lZ, chunksize)] = 1;
                    }
                }
            }
        }
        return to_increase;
    }

    /**
        Size height map: 4000 * 4000
        Chunk size 32

        region map 125 * 125, each value gives the height average of the chunk
    */
    public int[] build_region_map(SKBitmap height_map, int chunkSize, float data_width_per_pixel, int min_height_custom, int max_height_custom, int index, bool average=false){
        int regionToChunkRatio = height_map.Width/chunkSize;
        int[] regionMap = new int[regionToChunkRatio*regionToChunkRatio];

        for(int chunkX=0; chunkX < regionToChunkRatio; chunkX++){
            for(int chunkZ=0; chunkZ < regionToChunkRatio; chunkZ++){
                
                int averageLocalHeight = 0;
                int maximumLocalHeight = 0;

                for(int i=0; i < chunkSize; i++){
                    for(int j=0; j < chunkSize; j++){
                        int localHeight = (int) (min_height_custom + (max_height_custom - min_height_custom) * LerpPosHeight(chunkX*chunkSize + i - chunkSize/2, chunkZ*chunkSize + j - chunkSize/2, index, data_width_per_pixel, height_map));

                        if(average){
                            averageLocalHeight += localHeight;
                        }
                        else {
                            maximumLocalHeight = Math.Max(maximumLocalHeight, localHeight);
                        }
                        
                    }
                }

                int fakeChunkX = regionToChunkRatio-1-chunkX;
                int fakeChunkZ = regionToChunkRatio-1-chunkZ;

                //  For some reason, the data maps are transposed compared to the world map, hence the regionToChunkRatio - 1 - XXX
                if(average){
                    regionMap[ChunkIndex2d(fakeChunkX, fakeChunkZ, regionToChunkRatio)] = averageLocalHeight / (chunkSize*chunkSize);
                }
                else {
                    regionMap[ChunkIndex2d(fakeChunkX, fakeChunkZ, regionToChunkRatio)] = maximumLocalHeight;
                }
            }
        }
        return regionMap;
    }
    
    /**
        region map: 4000 * 4000
        Chunk size 32

        region map 125 * 125, each value gives the height average of the chunk
    */
    public int[] build_mini_region_map(IntDataMap2D referenceMap, int chunkX, int chunkZ, int regionToChunkRatio, int[] regionMap, int globalRegionSize, double ratio){
        /*
            Builds the local region map at the given chunk coordinates, at the referenceMap dimensions.
        */
        int regionSize = referenceMap.Size;
        int regionOffset = referenceMap.TopLeftPadding;
        
        int[] miniRegionMap = new int[regionSize*regionSize];

        for(int i = 0; i < miniRegionMap.Length; i++){
            int fakeChunkX = (int)(chunkX + (- mod(chunkX, globalRegionSize) - regionOffset + i%regionSize)*ratio);
            int fakeChunkZ = (int)(chunkZ + (- mod(chunkZ, globalRegionSize) - regionOffset + i/regionSize)*ratio);
            
            miniRegionMap[i] = regionMap[getOutBoundX(fakeChunkX, regionToChunkRatio) + regionToChunkRatio*getOutBoundX(fakeChunkZ, regionToChunkRatio)];
        }
        return miniRegionMap;
    }
    public int getRiverHeight(int worldX, int worldZ, int min_height_custom, int max_height_custom, float data_width_per_pixel, SKBitmap height_map){
        /*
            Takes the river height at the (worldX, worldZ) coordinates.

            Returns the minimum within a radius, to prevent artifacts at the water surface.
        */
        int radius = 2;
        int[] localRiverHeights = new int[(2*radius+1)*(2*radius+1)];

        for(int i=-radius; i<radius+1; i++){
            for(int j=-radius; j<radius+1; j++){
                if (LerpPosHeight(worldX + i, worldZ + j, 0, data_width_per_pixel, height_map) > 0){
                    localRiverHeights[i+radius+(j+radius)*(2*radius+1)] = (int) (min_height_custom + (max_height_custom - min_height_custom) * LerpPosHeight(worldX + i, worldZ + j, 0, data_width_per_pixel, height_map));
                }
                else{
                    localRiverHeights[i+radius+(j+radius)*(2*radius+1)] = max_height_custom;
                }
            }
        }

        return localRiverHeights.Min();
    }
    
    public void makeLakes(IServerChunk[] chunks, int chunkX, int chunkZ, int chunksize, int waterID, int gravelID, int min_height_custom, int max_height_custom, float data_width_per_pixel, SKBitmap height_map){
        /*
            Fills the potential lakes in the given chunk.
        */
        int lakeHeight;
        int groundHeight;
        
        for (int lZ = 0; lZ < chunksize*chunksize; lZ++){
            int worldX = chunkX * chunksize + lZ%chunksize+ offsetX;
            int worldZ = chunkZ * chunksize + lZ/chunksize + offsetZ;

            groundHeight = (int) (min_height_custom + (max_height_custom - min_height_custom) * LerpPosHeight(worldX, worldZ, 0, data_width_per_pixel, height_map));
            lakeHeight = (int) (min_height_custom + (max_height_custom - min_height_custom) * LerpPosHeight(worldX, worldZ, 1, data_width_per_pixel, height_map));

            if(lakeHeight > groundHeight){
                for(int posY = groundHeight; posY < lakeHeight; posY ++){
                    setBlockId(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks, waterID, fluid:true);
                }

                setBlockId(lZ%chunksize, groundHeight - 1, lZ/chunksize, chunksize, chunks, gravelID);
                setBlockId(lZ%chunksize, groundHeight - 2, lZ/chunksize, chunksize, chunks, gravelID);

                for(int posY = lakeHeight; posY < 30 + lakeHeight; posY ++){
                    SetBlockAir(lZ%chunksize, posY, lZ/chunksize, chunksize, chunks);
                }
            }
        }
    }    
    public void print(string str){
        /**
            Just a print
        */
        api.Logger.Notification(str);  
    }
}
struct ColumnResult
{
    public BitArray ColumnBlockSolidities;
}
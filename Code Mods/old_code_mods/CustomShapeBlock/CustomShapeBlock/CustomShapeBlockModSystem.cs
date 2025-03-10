using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace CustomShapeBlock
{
    /// <summary>
    /// Example on how to programmatically customize the shape of a block without adding an extra renderer to it
    /// Right click on the "customshape" block to have it randomly change shape
    /// </summary>
    public class CustomShapeBlock : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockClass("BlockCustomShape", typeof(BlockCustomShape));
            
            api.RegisterBlockEntityClass("CustomShape", typeof(BlockEntityCustomShape));
        }


        public class BlockCustomShape : Block
        {
            public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
            {
                BlockEntityCustomShape becs = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityCustomShape;

                if (becs != null)
                {
                    becs.Randomize();
                }

                return base.OnBlockInteractStart(world, byPlayer, blockSel);
            }
        }




        public class BlockEntityCustomShape : BlockEntity, ITexPositionSource
        {
            MeshData plane;
            TextureAtlasPosition texPosition;

            // Implentation of ITexPositionSource, this is not required to implement custom shapes though.
            public TextureAtlasPosition this[string textureCode] => texPosition;
            public Size2i AtlasSize => (Api as ICoreClientAPI).BlockTextureAtlas.Size;


            // Implementation of IBlockShapeSupplier
            public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
            {
                mesher.AddMeshData(plane);

                // We return so that the default block cube mesh is also added
                return false;
            }



            public override void Initialize(ICoreAPI api)
            {
                base.Initialize(api);

                if (api.Side == EnumAppSide.Client)
                {
                    ICoreClientAPI capi = api as ICoreClientAPI;
                    Block ownBlock = api.World.BlockAccessor.GetBlock(Pos);

                    // Loads the mesh from a model created by VS Model Creator, but you could just as well create a mesh in code, e.g. via QuadMeshUtil.GetCustomQuad();
                    Shape shape = capi.Assets.TryGet(new AssetLocation("customshapeblock", "shapes/customshapes/plane.json")).ToObject<Shape>();
                    texPosition = capi.BlockTextureAtlas.GetPosition(ownBlock, "north");
                    capi.Tesselator.TesselateShape("customshape", shape, out plane, this);

                    // Alternatively you can also use the block itself as texture souce and not implement ITexPositionSource
                    //capi.Tesselator.TesselateShape(ownBlock, shape, out plane);

                    // Move the model up by half a block. Alternatively move it up by half a block in the VSMC model
                    plane.Translate(0, 0.5f, 0);
                }
            }

            public void Randomize()
            {
                Random rnd = Api.World.Rand;

                // Randomize a bit
                plane.Translate(0, (float)(rnd.NextDouble() / 2 - 0.25), 0);
                plane.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, (float)(rnd.NextDouble() - 0.5), 0);

                // Now redraw the chunk
                MarkDirty(true);
            }
        }
    }
}
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace VSTutorial.BlockBehaviors
{

    /// <summary>
    /// BlockBehaviors are used to control certain aspects of a block. A block can have any number of behaviors, whereas it can only have a single class.
    /// It is generally better practice to use BlockBehaviors for anything that will be re-used throughout multiple blocks.
    /// Note that a new BlockBehavior class will be created for each blocktype that uses this - including independent variants.
    /// Also keep in mind that BlockBehaviors cannot store any data for in-game blocks. You will need to use a BlockEntity for that.
    /// </summary>
    internal class BlockBehaviorMoveable : BlockBehavior
    {
        /// <summary>
        /// The distance in blocks which the moving block should travel per click.
        /// </summary>
        public int distance = 1;

        /// <summary>
        /// Should we be able to pull the block by holding shift?
        /// </summary>
        public bool pull = false;

        public BlockBehaviorMoveable(Block block) : base(block)
        {}

        /// <summary>
        /// This function is used to initialize the properties of the block behavior. See the Block JSON asset to see how to add properties to the blocktype.
        /// </summary>
        /// <param name="properties"></param>
        public override void Initialize(JsonObject properties)
        {
            //This actually registers the properties so we can potentially use them later.
            base.Initialize(properties);

            //You can load properties by referencing the keys in JSON assets.
            distance = properties["distance"].AsInt(1);
            pull = properties["pull"].AsBool(false);
        }

        /// <summary>
        /// This function is called whenever the block with this behavior is right clicked on whilst holding anything.
        /// Note that this function has to decide whether to allow or not allow the default behavior of placing a block, or other interactions.
        /// </summary>
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            //A little bit of complex code to find the position.
            //Using the position of the block that is currently hovered over, we either add or subtract the distance based on the face of the block that is currently looked at.
            BlockPos pos = blockSel.Position.AddCopy(pull && byPlayer.WorldData.EntityControls.Sneak ? blockSel.Face : blockSel.Face.Opposite, distance);

            //Using IsReplacableBy allows us to see if the block can replace whatever is in the new position (either air, grass, or something else replacable).
            if (world.BlockAccessor.GetBlock(pos).IsReplacableBy(block))
            {
                //First, set the block ID to 0 to change it into air.
                world.BlockAccessor.SetBlock(0, blockSel.Position);
                //Now we replace the block at the new position.
                world.BlockAccessor.SetBlock(block.BlockId, pos);
            }
            //Notify the game engine other block behaviors that we handled the players interaction with the block.
            //If we would not set the handling field the player would still be able to place blocks if he has them in hands.
            handling = EnumHandling.PreventDefault;
            return true;
        }

    }
}

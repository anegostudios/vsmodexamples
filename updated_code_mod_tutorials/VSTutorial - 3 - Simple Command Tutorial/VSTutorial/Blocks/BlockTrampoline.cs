//Here are the imports for this script. Most of these will add automatically.
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

/*
* The namespace the class will be in. This is essentially the folder the script is found in.
* If you need to use the BlockTrampoline class in any other script, you will have to add 'using VSTutorial.Blocks' to that script.
* Note that you can instead write 'namespace VSTutorial.Blocks;' on a single line and this removes the need for the { }'s after.
*/
namespace VSTutorial.Blocks
{
    /*
    * The class definition. Here, you define BlockTrampoline as a child of Block, which
    * means you can 'override' many of the functions within the general Block class. 
    * Take a look at https://apidocs.vintagestory.at/api/Vintagestory.API.Common.Block.html#methods and
    *   https://apidocs.vintagestory.at/api/Vintagestory.API.Common.CollectibleObject.html#methods for all the methods that can be overriden.
    */
    internal class BlockTrampoline : Block
    {
        /*
         * This function is called when any entity collides with this block. Here you can implement the 'bounce' functionality.
         * From here, you can access the world, the entity, the block's position, and details about the collision.
         */
        public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
        {
            //The entity should only 'bounce' if this an impact and the entity came from above (or below).
            if (isImpact && facing.IsVertical)
            {
                /*
                 * This is how to access the entity's motion.
                 * You can reverse the velocity by multiplying by a negative, and then reduce it to 80% to lose some speed.
                 */
                entity.Pos.Motion.Y *= -0.8f;
            }
        }
    }
}
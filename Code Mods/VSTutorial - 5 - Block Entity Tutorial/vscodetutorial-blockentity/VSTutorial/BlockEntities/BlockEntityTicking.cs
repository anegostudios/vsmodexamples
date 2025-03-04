using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace VSTutorial
{
    /// <summary>
    /// BlockEntities are used to add logic to independent placed blocks. It allows each placed block of a certain kind to have different interactions and store data.
    /// This BlockEntity will use a timer to swap between two block types. The timer will also be saved using the To and FromTreAttributes functions.
    /// </summary>
    public class BlockEntityTicking : BlockEntity
    {
        /// <summary>
        /// This is the timer that controls how long the block has been in existence for.
        /// </summary>
        public float timer;

        /// <summary>
        /// Used to initialize the block. We can create tick listeners here and initialize values.
        /// This is when the block is created in the world for any reason, whether it was placed by the player, placed by worldgen, or loaded from a saved game.
        /// </summary>
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            /*
             * GameTickListeners are exceptionally useful when interacting with the world. 
             * Since it's inefficient to run a function on every frame, the game offers an time-based event listener that a BlockEntities and other objects can do.
             * In this case, we ask the game to call the 'OnGameTick' function every 50ms.
             * When our block is removed, the game tick listener will automatically be disposed. 
             *  Although listeners can be disposed of using their ID, there is no need to dispose of it yourself in a BlockEntity.
             */
            RegisterGameTickListener(OnGameTick, 50);
        }

        /// <summary>
        /// This is the function that is called by the game tick listener noted above, every 50ms.
        /// </summary>
        /// <param name="dt">Delta time. This is the number of seconds since the last time the function was called.</param>
        public void OnGameTick(float dt)
        {
            //Note that it may be temping to do timer += 0.05, since the function should be called every 50ms, however this is not always the case.
            // Due to framerates and stuff like that, dt may not always be 0.05, so it is important to use the dt parameter to ensure our timer actually remains in sync.
            timer += dt;

            //Rather simple. If the elapsed time is more than 3 seconds, use the block accessor to swap the block depending if it is currently on or off.
            if(timer >= 3)
            {
                Block block = Api.World.BlockAccessor.GetBlock(Pos);
                if (block.Code.Path.EndsWith("-on"))
                    block = Api.World.GetBlock(block.CodeWithParts("off"));
                else
                    block = Api.World.GetBlock(block.CodeWithParts("on"));
                Api.World.BlockAccessor.SetBlock(block.BlockId, Pos);
            }
        }

        /// <summary>
        /// This function is called when the block is being saved. You can add elements to the tree attribute to ensure that they can be saved and then loaded.
        /// </summary>
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            //The base method *must* be called for data to properly save.
            base.ToTreeAttributes(tree);

            //Tree attributes use a variety of 'Set...' and 'Get...' functions to save and load data.
            tree.SetFloat("timer", timer);
        }

        /// <summary>
        /// This function is called when the block is loading. You can load elements using the same keys that were used to save them.
        /// </summary>
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            //The base method *must* be called for data to properly load.
            base.FromTreeAttributes(tree, worldAccessForResolve);

            //Tree attributes use a variety of 'Set...' and 'Get...' functions to save and load data.
            timer = tree.GetFloat("timer");
        }
    }
}
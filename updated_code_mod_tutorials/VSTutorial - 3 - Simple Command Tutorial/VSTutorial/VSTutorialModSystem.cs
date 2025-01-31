using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSTutorial.Blocks;
using VSTutorial.Commands;
using VSTutorial.Items;

namespace VSTutorial
{
    /*
     * This is the entry point for the mod. ModSystems will be automatically detected and contain a load of useful functions for loading mods.
     * Take a look at https://apidocs.vintagestory.at/api/Vintagestory.API.Common.ModSystem.html for more info.
     */    
    public class VSTutorialModSystem : ModSystem
    {
        /*
         * This function is automatically called on the server and client when a world is loaded.
         * Here you can register any blocks, items, entities, and other stuff that needs to be on  both the client and server sides.
         */
        public override void Start(ICoreAPI api)
        {
            /*
             * This registers a new block class to be used in the game.
             * When registering anything, the name should be "{modid}.{objectname}" in lowercase. This helps avoid duplicate IDs when using multiple mods.
             * After registering, any blocks with the property: ' ("class":"vstutorial.trampoline") ' will use the BlockTrampoline class.
             */
            api.RegisterBlockClass(Mod.Info.ModID + ".trampoline", typeof(BlockTrampoline));

            /*
             * This is very similar to the line above, however this registers an item class.
             */
            api.RegisterItemClass(Mod.Info.ModID + ".thornsblade", typeof(ItemThornsBlade));   
        }

        /// <summary>
        /// This function is automatically called only on the server when a world is loaded.
        /// It is often used to load server-side configs, or create server-side commands.
        /// </summary>
        /// <param name="api"></param>
        public override void StartServerSide(ICoreServerAPI api)
        {
            //This will register your server commands.
            VSTutorialCommands.RegisterServerCommands(api);
        }

        /// <summary>
        /// This function is automatically called only on the client when a world is loaded.
        /// It is often used to create rendering mechanics, or create client-side commands.
        /// </summary>
        /// <param name="api"></param>
        public override void StartClientSide(ICoreClientAPI api)
        {
            //Although nothing is added to it, this will register your defined client commands.
            VSTutorialCommands.RegisterClientCommands(api);
        }
    }
}

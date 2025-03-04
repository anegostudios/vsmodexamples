using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSTutorial.Commands;

namespace VSTutorial
{
    /*
     * This is the entry point for the mod. ModSystems will be automatically detected and contain a load of useful functions for loading mods.
     * Take a look at https://apidocs.vintagestory.at/api/Vintagestory.API.Common.ModSystem.html for more info.
     */    
    public class VSTutorialModSystem : ModSystem
    {
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

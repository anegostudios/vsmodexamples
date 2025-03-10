    using Vintagestory.API.Client;
using Vintagestory.API.Common;
using HarmonyLib;

namespace VSTutorial
{
    /*
     * This is the entry point for the mod. ModSystems will be automatically detected and contain a load of useful functions for loading mods.
     * Take a look at https://apidocs.vintagestory.at/api/Vintagestory.API.Common.ModSystem.html for more info.
     */    
    public class VSTutorialModSystem : ModSystem
    {
        /// <summary>
        /// An instance of the harmony patcher.
        /// </summary>
        private Harmony patcher;

        /*
         * This function is automatically called on the server and client when a world is loaded.
         * Here you can register any blocks, items, entities, and other stuff that needs to be on  both the client and server sides.
         */
        public override void Start(ICoreAPI api)
        {
            //If the client and server run from the same instance, there's a chance that without this check the patches will exist twice.
            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                //Create our harmony patcher, using our mod ID as a unique ID.
                patcher = new Harmony(Mod.Info.ModID);
                //PatchCategory will look for any [HarmonyPatchCategory("vstutorial")] classes, and patch them. 
                patcher.PatchCategory(Mod.Info.ModID);
            }
        }

        /// <summary>
        /// This function is called when our mod is unloaded - Either when the game closes or a world is exited.
        /// </summary>
        public override void Dispose()
        {
            //It's important to remove our patches when disposed, otherwise any worlds loaded after closing would still contain the patches even if the mod was disabled.
            patcher?.UnpatchAll(Mod.Info.ModID);
        }

    }
}

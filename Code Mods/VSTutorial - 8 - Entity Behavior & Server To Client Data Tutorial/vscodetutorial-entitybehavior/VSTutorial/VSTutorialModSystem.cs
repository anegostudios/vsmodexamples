using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSTutorial.EntityBehaviors;

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
             * This registers a new entity behavior class to be used in the game.
             * When registering anything, the name should be "{modid}.{objectname}" in lowercase. This helps avoid duplicate IDs when using multiple mods.
             * After registering, any entities with the following property block will use the EntityBehaviorTotalPlayTime behavior:
               "behaviors": 
                [
                    {
                        "code": "repulseagents"
                    },
                ]
             * Note that behaviors need to be added on both sides in the entity JSON file. In this mod, the behaviors are added to the player entity using a patch.
             */
            api.RegisterEntityBehaviorClass(Mod.Info.ModID + ".totalplaytime", typeof(EntityBehaviorTotalPlayTime));
        }

        /// <summary>
        /// Using client side to create a client-side command...
        /// </summary>
        public override void StartClientSide(ICoreClientAPI api)
        {
            //This is a very quick way of writing a command - Useful for simple in-line commands.
            //In this instance, doing ".playtime" in-game will access the entity behavior, and get the total time played for, and display it.
            //  Remember that our TotalTimePlayedFor was only ever set on the server.
            //  This is just demonstrating that the value is synced with the client.
            api.ChatCommands.Create("playtime")
                .RequiresPlayer()
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(args =>
                {
                    double hoursPlayed = args.Caller.Player.Entity.GetBehavior<EntityBehaviorTotalPlayTime>().TotalTimePlayedFor;
                    return TextCommandResult.Success("You have existed in this world for " + hoursPlayed + " in-game hours.");
                });
        }
    }
}
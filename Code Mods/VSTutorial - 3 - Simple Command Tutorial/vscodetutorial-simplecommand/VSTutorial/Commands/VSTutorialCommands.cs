using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace VSTutorial.Commands
{
    /*
     * This class is simply going to store some command-related functions, so it doesn't need to extend from anything.
     * Because it's just for functions, we're going to make it static.
     *  This means that you cannot use 'new VSTutorialCommands()' to create an instance, but instead you use VSTutorialCommands.SetupCommands.
     *  Also note that all functions in this class also have to be static.
     */
    internal static class VSTutorialCommands
    {
        /// <summary>
        /// You can use this function to register your server-side commands.
        /// All server-side commands are used by typing '/command'.
        /// Server-side commands are often used for anything that modifies any data in a world.
        /// </summary>
        /// <param name="api"></param>
        public static void RegisterServerCommands(ICoreServerAPI api)
        {
            /*
             * Registering commands uses a 'builder' pattern. There are a number of functions that can be added to a single command,
             *     without requiring you to store the object in a variable.
             */
            //The argument given in the create function will be how the command is called in-game.
            api.ChatCommands.Create("spawn")
                //This is a description of the command.
                .WithDescription("Teleport to your spawn point.")
                //This says that the command requires a player to use it - Not through a console.
                .RequiresPlayer()
                //The command requires the 'chat' permission. In essence, anyone can use the command.
                .RequiresPrivilege(Privilege.chat)
                //The command should be handled by the 'HandleSpawnCommand' function.   
                .HandleWith(HandleSpawnCommand);
        }

        /// <summary>
        /// Although left empty, this function is where you can register client-side commands.
        /// These commands are used not with '/command', but with '.command'.
        /// Client commands are recommended for displaying information that is exclusively on the client side.
        ///     If you need to access server data, it is recommended to use a server-side command.
        /// </summary>
        /// <param name="api"></param>
        public static void RegisterClientCommands(ICoreClientAPI api) { }
        
        /// <summary>
        /// Every command we register should have a handler function. 
        /// When the command is used, this function will execute the command and determine if it was successful or not. 
        /// In this particular instance, the command should teleport the user to their spawnpoint.
        /// </summary>
        private static TextCommandResult HandleSpawnCommand(TextCommandCallingArgs args)
        {
            /* 
             * First, the spawn position needs to be accessed. Passing 'false' here ensures that the players respawn uses (with a temporal gear) are not consumed.
             * Casting args.Caller.Player to the IServerPlayer type gives us access to more data.
             *    Note you can only do this on the server, but since the command is server-side, this is fine.
             */
            FuzzyEntityPos spawnPosition = (args.Caller.Player as IServerPlayer).GetSpawnPosition(false);
            
            //Then, the player can be teleported to this position.
            args.Caller.Player.Entity.TeleportTo(spawnPosition);

            //The command was successfully executed, so the function returns TextCommandResult.Success().
            return TextCommandResult.Success();
        }

    }
}

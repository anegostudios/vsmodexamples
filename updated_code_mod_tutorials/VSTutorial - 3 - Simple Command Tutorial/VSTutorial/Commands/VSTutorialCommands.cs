using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
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
            //The argument given in the Create function will be how the command is called in-game.
            api.ChatCommands.Create("sell")
                .WithDescription("");
        }

        /// <summary>
        /// Although left empty, this function is where you can register client-side commands.
        /// These commands are used not with '/command', but with '.command'.
        /// Client commands are recommended for displaying information that is exclusively on the client side.
        ///     If you need to access server data, it is recommended to use a server-side command.
        /// </summary>
        /// <param name="api"></param>
        public static void RegisterClientCommands(ICoreClientAPI api)
        {

        }
        
        /// <summary>
        /// Every command we register should have a handler function. 
        /// When the command is used, this function will execute the command and determine if it was successful or not. 
        /// </summary>
        private static TextCommandResult HandleCommand(TextCommandCallingArgs args)
        {
            //args.Caller.Player.InventoryManager.ActiveHotbarSlot.Itemstack = new ItemStack()
            return TextCommandResult.Success();
        }

    }
}

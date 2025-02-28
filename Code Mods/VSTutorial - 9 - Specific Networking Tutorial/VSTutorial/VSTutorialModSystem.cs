using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSTutorial.Networking;

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
            //The network channel and message types need to be registered here.
            //Not much to it, other than ensuring that the 'register channel' ID is unique.
            //You'll need to list all your ProtoBuf networked classes here so they can be sent/received.
            api.Network.RegisterChannel(Mod.Info.ModID + ".networkchannel")
                .RegisterMessageType<VSTutorialNetworkMessage>()
                .RegisterMessageType<VSTutorialNetworkResponse>();
        }

        /// <summary>
        /// This function is automatically called only on the server when a world is loaded.
        /// It is often used to load server-side configs, or create server-side commands.
        /// </summary>
        /// <param name="api"></param>
        public override void StartServerSide(ICoreServerAPI api)
        {
            serverChannel = api.Network.GetChannel("networkapitest")
                .SetMessageHandler<NetworkApiTestResponse>(OnClientMessage)
            ;

            api.ChatCommands.Create("nwtest")
                .WithDescription("Send a test network message")
                .RequiresPrivilege(Privilege.controlserver)
                .HandleWith(new OnCommandDelegate(OnNwTestCmd));
        }

        /// <summary>
        /// This function is automatically called only on the client when a world is loaded.
        /// It is often used to create rendering mechanics, or create client-side commands.
        /// </summary>
        /// <param name="api"></param>
        public override void StartClientSide(ICoreClientAPI api)
        {

        }
    }
}

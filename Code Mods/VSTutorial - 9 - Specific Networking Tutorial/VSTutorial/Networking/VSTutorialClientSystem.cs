using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VSTutorial.Networking
{
    /// <summary>
    /// Another mod system, this one is going to control all of our client-side logic for the networking!
    /// </summary>
    internal class VSTutorialClientSystem : ModSystem
    {
        /// <summary>
        /// The client-side network channel. This is registered in <see cref="VSTutorialModSystem"/>, and then later accessed here.
        /// </summary>
        IClientNetworkChannel clientChannel;

        /// <summary>
        /// The client-side API.
        /// </summary>
        ICoreClientAPI clientApi;

        /// <summary>
        /// This is a client-side system, so we only want it to load on the client.
        /// </summary>
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        /// <summary>
        /// This mod system needs to be loaded after the <see cref="VSTutorialModSystem"/> to ensure that the network channel is registered, 
        ///   so we have to give this a slightly higher execute order.
        /// </summary>
        public override double ExecuteOrder()
        {
            //Default is 0.1.
            return 0.11f;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            //We need the client api for later...
            clientApi = api;

            //... and the client channel can be accessed using the same ID in the main mod system.
            clientChannel = api.Network.GetChannel(Mod.Info.ModID + ".networkchannel");

            //Finally, let the client channel to listen out for a network message from the server, and register a function when we receive one.
            clientChannel.SetMessageHandler<VSTutorialNetworkMessage>(OnReceivedMessageFromServer);
        }

        /// <summary>
        /// This function is called by the client channel, when we receive a <see cref="VSTutorialNetworkMessage"/>.
        /// </summary>
        private void OnReceivedMessageFromServer(VSTutorialNetworkMessage networkMessage)
        {
            // The network message that was sent to the client can now be read. In this case, let's send a message to the client.
            clientApi.ShowChatMessage("Received following message from server: " + networkMessage.message);
            clientApi.ShowChatMessage("Sending response.");

            // And use the clientChannel to send a response packet to the server.
            clientChannel.SendPacket(new VSTutorialNetworkResponse()
            {
                response = "RE: Hello World!"
            });
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSTutorial.Networking
{
    /// <summary>
    /// Another mod system, this one is going to control all of our server-side logic for the networking!
    /// </summary>
    internal class VSTutorialServerSystem : ModSystem
    {
        IServerNetworkChannel serverChannel;
        ICoreServerAPI sapi;

        /// <summary>
        /// This is a server-side system, so we do not want it to load on the client.
        /// </summary>
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide != EnumAppSide.Client;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            serverChannel = api.Network.GetChannel(Mod.Info.ModID + ".networkchannel")
                .SetMessageHandler<VSTutorialNetworkResponse>(OnClientMessage)
            ;
            sapi = api;
            api.ChatCommands.Create("nwtest")
                .WithDescription("Send a test network message")
                .RequiresPrivilege(Privilege.controlserver)
                .HandleWith(new OnCommandDelegate(OnNwTestCmd));
        }

        private TextCommandResult OnNwTestCmd(TextCommandCallingArgs args)
        {
            serverChannel.BroadcastPacket(new VSTutorialNetworkMessage()
            {
                message = "Hello World!",
            });
            return TextCommandResult.Success();
        }

        private void OnClientMessage(IPlayer fromPlayer, VSTutorialNetworkResponse networkMessage)
        {
            sapi.SendMessageToGroup(
                GlobalConstants.GeneralChatGroup,
                "Received following response from " + fromPlayer.PlayerName + ": " + networkMessage.response,
                EnumChatType.Notification
            );
        }

    }
}

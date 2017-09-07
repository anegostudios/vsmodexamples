using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace Vintagestory.ServerMods
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NetworkApiTestMessage
    {
        public string message;
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class NetworkApiTestResponse
    {
        public string response;
    }

    /// <summary>
    /// A basic example of client<->server networking using a custom network communication
    /// </summary>
    public class NetworkApiTest : ModBase
    {
        #region Client
        IClientNetworkChannel clientChannel;
        ICoreClientAPI clientApi;

        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;

            clientChannel =
                api.Network.RegisterChannel("networkapitest")
                .RegisterMessageType(typeof(NetworkApiTestMessage))
                .RegisterMessageType(typeof(NetworkApiTestResponse))
                .SetMessageHandler<NetworkApiTestMessage>(OnServerMessage)
            ;
        }

        private void OnServerMessage(NetworkApiTestMessage networkMessage)
        {
            clientApi.ShowChatNotification("Received following message from server: " + networkMessage.message);
            clientApi.ShowChatNotification("Sending response.");
            clientChannel.SendPacket(new NetworkApiTestResponse()
            {
                response = "RE: Hello World!"
            });
        }

        #endregion

        #region Server
        IServerNetworkChannel serverChannel;
        ICoreServerAPI serverApi;

        public override void StartServerSide(ICoreServerAPI api)
        {
            serverApi = api;

            serverChannel =
                api.Network.RegisterChannel("networkapitest")
                .RegisterMessageType(typeof(NetworkApiTestMessage))
                .RegisterMessageType(typeof(NetworkApiTestResponse))
                .SetMessageHandler<NetworkApiTestResponse>(OnClientMessage)
            ;

            api.RegisterCommand("nwtest", "Send a test network message", "", OnNwTestCmd, Privilege.controlserver);
        }

        private void OnNwTestCmd(IServerPlayer player, int groupId, CmdArgs args)
        {
            serverChannel.BroadcastPacket(new NetworkApiTestMessage()
            {
                message = "Hello World!",
            });
        }

        private void OnClientMessage(IPlayer fromPlayer, NetworkApiTestResponse networkMessage)
        {
            serverApi.SendMessageToGroup(
                GlobalConstants.GeneralChatGroup,
                "Received following response from " + fromPlayer.PlayerName + ": " + networkMessage.response,
                EnumChatType.Notification
            );
        }
        #endregion 
    }
}

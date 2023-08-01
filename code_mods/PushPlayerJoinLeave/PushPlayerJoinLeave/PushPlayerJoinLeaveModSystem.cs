using System.Collections.Generic;
using System.Net.Http;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PushPlayerJoinLeave
{

    /// <summary>
    /// Pushes player join/leave events to a predefined url
    /// </summary>
    public class PushPlayerJoinLeave : ModSystem
    {
        PushConfig config = new PushConfig();
        

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            try
            {
                config = api.LoadModConfig<PushConfig>("pushconfig.json");
            }
            catch {
                api.Server.LogWarning("Failed reading pushconfig.json");
                return;
            }

            if (config.Url != null && config.Url.Length > 0)
            {
                api.Event.PlayerJoin += OnJoin;
                api.Event.PlayerDisconnect += OnLeave;

                api.Server.LogNotification("Will push join/leave messages to " + config.Url);
            } else
            {
                api.Server.LogNotification("No push url provided, won't push join/leave messages");
            }
        }

        private void OnLeave(IServerPlayer byPlayer)
        {
            PushMessage(byPlayer.PlayerName + " left the public game server.");
        }

        private void OnJoin(IServerPlayer byPlayer)
        {
            PushMessage(byPlayer.PlayerName + " joined the public game server.");
        }


        void PushMessage(string message)
        {
            HttpClient client = new HttpClient();

            var values = new Dictionary<string, string>
            {
               { "api_key", config.ApiKey },
               { "channel", config.Channel },
               { "message", message }
            };

            var content = new FormUrlEncodedContent(values);
            client.PostAsync(config.Url, content);
        }
    }
}

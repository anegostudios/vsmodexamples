using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace PushPlayerJoinLeave
{
    public class PushConfig
    {
        public string Url;
        public string Channel;
        public string ApiKey;
    }

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
                string configtext = File.ReadAllText(Path.Combine(api.DataBasePath, "pushconfig.json"));
                config = JsonConvert.DeserializeObject<PushConfig>(configtext);
            }
            catch {
                api.Server.LogNotification("Failed reading pushconfig.json");
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

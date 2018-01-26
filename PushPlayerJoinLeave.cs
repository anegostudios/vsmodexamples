using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VSExampleMods
{
    public class PushConfig
    {
        public string Url;
        public string Channel;
        public string ApiKey;
    }

    public class PushPlayerJoinLeave : ModBase
    {
        PushConfig config = new PushConfig();

        public override ModInfo GetModInfo()
        {
            return new ModInfo()
            {
                Author = "Tyron",
                Version = "1",
                Description = "Pushes player join/leave events to a predefined url",
                Name = "PushJoinLeave"
            };
        }

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Event.PlayerJoin(OnJoin);
            api.Event.PlayerLeave(OnLeave);

            string configtext = File.ReadAllText(Path.Combine(api.DataBasePath, "pushconfig.json"));
            try
            {
                config = JsonConvert.DeserializeObject<PushConfig>(configtext);
            } catch { }

            api.Server.LogNotification("Will push join/leave messages to " + config.Url);
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

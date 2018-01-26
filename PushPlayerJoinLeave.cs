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
            PushMessage(byPlayer.PlayerName + " left.");
        }

        private void OnJoin(IServerPlayer byPlayer)
        {
            PushMessage(byPlayer.PlayerName + " joined.");
        }


        void PushMessage(string message)
        {
            HttpClient client = new HttpClient();

            string json = string.Format(
                "{ \"api_key\": \"{0}\", \"message\": \"{1}\" }", 
                config.ApiKey, config.Url
            );

            var values = new Dictionary<string, string>
            {
               { "data", json },
            };

            var content = new FormUrlEncodedContent(values);
            client.PostAsync(config.Url, content);
        }
    }
}

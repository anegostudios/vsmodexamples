using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VSExampleMods
{
    /// <summary>
    /// Redirects all log entries into the visual studio output window. Only for your convenience during development and testing.
    /// </summary>
    public class RedirectLogs : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide side)
        {
            return true;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Server.Logger.EntryAdded += OnServerLogEntry;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.World.Logger.EntryAdded += OnClientLogEntry;
        }

        private void OnClientLogEntry(EnumLogType logType, string message, object[] args)
        {
            if (logType == EnumLogType.VerboseDebug) return;
            System.Diagnostics.Debug.WriteLine("[Client " + logType + "] " + message, args);
        }

        private void OnServerLogEntry(EnumLogType logType, string message, object[] args)
        {
            if (logType == EnumLogType.VerboseDebug) return;
            System.Diagnostics.Debug.WriteLine("[Server " + logType + "] " + message, args);
        }
    }
}

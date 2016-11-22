using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;

namespace VSExampleMods
{
    /// <summary>
    /// Redirects all log entries into the visual studio output window. Only for your convenience during development and testing.
    /// </summary>
    public class RedirectLogs : ModBase
    {
        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Server.Logger.AddListener(OnServerLogEntry);
        }

        private void OnServerLogEntry(EnumLogType logType, string message, object[] args)
        {
            if (logType == EnumLogType.VerboseDebug) return;

            System.Diagnostics.Debug.WriteLine("[Server " + logType + "] " + message, args);
        }
    }
}

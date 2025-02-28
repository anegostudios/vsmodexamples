using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace VSTutorial.Networking
{
    /// <summary>
    /// Another mod system, this one is going to control all of our server-side logic for the networking!
    /// </summary>
    internal class VSTutorialServerSystem : ModSystem
    {

        /// <summary>
        /// This is a server-side system, so we do not want it to load on the client.
        /// </summary>
        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide != EnumAppSide.Client;
        }

    }
}

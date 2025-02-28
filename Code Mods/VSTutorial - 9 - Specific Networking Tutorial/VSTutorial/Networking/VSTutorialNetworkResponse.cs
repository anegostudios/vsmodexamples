using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTutorial.Networking
{
    [ProtoContract]
    internal class VSTutorialNetworkResponse
    {
        [ProtoMember(1)]
        public string response;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace ILSnowballFight
{
    public class PlayerCache
    {
        public NetConnection connection { get; private set; }
        public bool inLobby = true;
        public int roomID;

        public PlayerCache(NetConnection connection)
        {
            this.connection = connection;
        }
    }
}

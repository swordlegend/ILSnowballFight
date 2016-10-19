using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace ILSnowballFight
{
    public static class PlayerCaches
    {
        static Dictionary<NetConnection, PlayerCache> caches = new Dictionary<NetConnection, PlayerCache>();

        public static bool AuthDone(NetConnection connection)
        {
            return caches.ContainsKey(connection);
        }

        public static void AddPlayerCache(NetConnection connection)
        {
            caches.Add(connection, new PlayerCache(connection));
        }

        public static bool InLobby(NetConnection connection)
        {
            return caches[connection].inLobby;
        }

        public static void EnterRoom(NetConnection connection, int roomid)
        {
            caches[connection].inLobby = false;
            caches[connection].roomID = roomid;
        }

        public static void BackToLobby(NetConnection connection)
        {
            caches[connection].inLobby = true;
        }

        public static int GetRoomID(NetConnection connection)
        {
            return caches[connection].roomID;
        }

        public static void Disconnect(NetConnection connection)
        {
            caches.Remove(connection);
        }
    }
}

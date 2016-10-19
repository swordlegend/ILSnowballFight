using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YasGameLib;

namespace ILSnowballFight
{
    public static class Lobby
    {
        static Dictionary<NetConnection, Player> players = new Dictionary<NetConnection, Player>();
        static List<Room> rooms = new List<Room>();

        public static void Init()
        {
            for(int i=0; i<Env.RoomNum; i++)
            {
                rooms.Add(new Room(i));
            }
        }

        public static int AddPlayer(NetConnection connection, string username)
        {
            Player player = new Player(connection, username);
            players.Add(connection, player);

            PlayerCaches.AddPlayerCache(connection);

            return player.userid;
        }

        public static LobbyState GetLobbyState()
        {
            LobbyState state = new LobbyState();
            foreach(Room room in rooms)
            {
                state.summaries.Add(room.GetRoomSummary());
            }

            return state;
        }

        public static void EnterRoom(NetConnection connection, int roomid)
        {
            Player player = players[connection];
            players.Remove(connection);
            rooms[roomid].EnterRoom(player);
        }

        public static void SendRoomStateToAll(int roomid)
        {
            rooms[roomid].SendRoomStateToAll();
        }

        public static int GetRoomState(int roomid)
        {
            return rooms[roomid].GetRoomState();
        }

        public static void LeaveRoom(NetConnection connection, int roomid)
        {
            Player player = rooms[roomid].LeaveRoom(connection);
            players.Add(connection, player);
            PlayerCaches.BackToLobby(connection);
        }

        public static void ChangeFaction(NetConnection connection, int roomid)
        {
            rooms[roomid].ChangeFaction(connection);
        }

        public static void ChangeReady(NetConnection connection, int roomid)
        {
            rooms[roomid].ChangeReady(connection);
        }

        public static void CheckBattleStart(int roomid)
        {
            rooms[roomid].CheckBattleStart();
        }

        public static void Push(NetConnection connection, int roomid, PushData data)
        {
            rooms[roomid].Push(connection, data);
        }

        public static void SendSnapShot()
        {
            foreach(Room room in rooms)
            {
                room.SendSnapShot();
            }
        }

        public static void Update(float delta)
        {
            foreach (Room room in rooms)
            {
                room.Update(delta);
            }
        }

        public static void TouchCore(NetConnection connection, int roomid)
        {
            rooms[roomid].TouchCore(connection);
        }

        public static void ReplySetBlock(int roomid, BlockData block)
        {
            rooms[roomid].ReplySetBlock(block);
        }

        public static void ReplyFire(NetConnection connection, int roomid, FireData fire)
        {
            rooms[roomid].ReplyFire(connection, fire);
        }

        public static void Disconnect(NetConnection connection)
        {
            if (PlayerCaches.InLobby(connection))
            {
                Console.WriteLine("Leave:" + players[connection].username);
                players.Remove(connection);
            }
            else
            {
                int roomid = PlayerCaches.GetRoomID(connection);
                rooms[roomid].Disconnect(connection);
            }

            PlayerCaches.Disconnect(connection);
        }
    }
}

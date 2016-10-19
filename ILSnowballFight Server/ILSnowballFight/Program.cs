using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YasGameLib;

namespace ILSnowballFight
{
    class Program
    {
        static Task task;
        static bool exit = false;
        const float frameSpan = 1.0f / 30.0f;
        static int sendCount = 0;

        static int port;
        static string sqlpath;

        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory() + "/Setting/setting.txt";
            if (!ReadFile(path))
            {
                return;
            }

            task = Task.Run(() => {
                Server();
            });

            while (true)
            {
                Console.WriteLine(@"type exit for close server");
                string input = Console.ReadLine();
                if (input == "exit")
                {
                    break;
                }
            }
            exit = true;

            task.Wait();
        }

        static void Server()
        {
            Lobby.Init();

            //GSQLite.Open(sqlpath);            

            GSrv.Init();
            GSrv.SetConnectPacketHandler(ConnectHandler);
            GSrv.SetDisconnectPacketHandler(DisconnectHandler);
            GSrv.SetDebugPacketHandler(DebugHandler);
            GSrv.SetPacketHandler(MessageType.SendUserName, DataType.String, SendUserNameHandler);
            GSrv.SetPacketHandler(MessageType.RequestLobbyState, DataType.Null, RequestLobbyStateHandler);
            GSrv.SetPacketHandler(MessageType.EnterRoom, DataType.Int32, EnterRoomHandler);
            GSrv.SetPacketHandler(MessageType.LeaveRoom, DataType.Null, LeaveRoomHandler);
            GSrv.SetPacketHandler(MessageType.ChangeFaction, DataType.Null, ChangeFactionHandler);
            GSrv.SetPacketHandler(MessageType.ChangeReady, DataType.Null, ChangeReadyHandler);
            GSrv.SetPacketHandler(MessageType.Push, DataType.Bytes, PushHandler);
            GSrv.SetPacketHandler(MessageType.TouchCore, DataType.Null, TouchCoreHandler);
            GSrv.SetPacketHandler(MessageType.SetBlock, DataType.Bytes, SetBlockHandler);
            GSrv.SetPacketHandler(MessageType.SendFire, DataType.Bytes, SendFireHandler);

            GSrv.Listen("ILSnowballFight0.1", port);

            while (!exit)
            {
                DateTime startTime = DateTime.Now;

                GSrv.Receive();

                sendCount++;
                
                if (sendCount == 3)
                {
                    sendCount = 0;

                    SendSnapshot();
                }

                Update(frameSpan);

                TimeSpan span = DateTime.Now - startTime;
                if (span.TotalMilliseconds < frameSpan * 1000)
                {
                    Thread.Sleep((int)(frameSpan * 1000) - (int)span.TotalMilliseconds);
                }
            }

            GSrv.Shutdown();
            /*Players.SaveAllPlayer();
            GSQLite.Close();*/
        }

        static void SendSnapshot()
        {
            Lobby.SendSnapShot();
        }

        static void Update(float delta)
        {
            Lobby.Update(delta);
        }

        static bool ReadFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            try
            {
                using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
                {
                    while (true)
                    {
                        string line = sr.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        if (line.StartsWith("Port="))
                        {
                            string sub = line.Substring("Port=".Length);
                            if (!int.TryParse(sub, out port))
                            {
                                throw new Exception();
                            }
                        }
                        else if (line.StartsWith("SQLite="))
                        {
                            string sub = line.Substring("SQLite=".Length);
                            sqlpath = Directory.GetCurrentDirectory() + "/SQLite/" + sub;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        static public void ConnectHandler(NetConnection connection, object data)
        {
            
        }

        static public void DisconnectHandler(NetConnection connection, object data)
        {
            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            Lobby.Disconnect(connection);
        }

        static public void DebugHandler(NetConnection connection, object data)
        {
            Console.WriteLine((string)data);
        }

        static public void PushHandler(NetConnection connection, object data)
        {
            PushData push = null;
            try
            {
                push = GSrv.Deserialize<PushData>((byte[])data);
            }
            catch
            {
                return;
            }

            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            if (PlayerCaches.InLobby(connection))
            {
                return;
            }

            int roomid = PlayerCaches.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 0)
            {
                return;
            }

            Lobby.Push(connection, roomid, push);
        }

        static public void SendUserNameHandler(NetConnection connection, object data)
        {
            string username = (string)data;
            if(username == "")
            {
                username = "Anonymouse";
            }

            if(PlayerCaches.AuthDone(connection))
            {
                return;
            }

            Console.WriteLine("Enter:" + username);

            int userid = Lobby.AddPlayer(connection, username);
            GSrv.Send(MessageType.ReplyUserID, userid, connection, NetDeliveryMethod.ReliableOrdered);
            LobbyState state = Lobby.GetLobbyState();
            GSrv.Send(MessageType.ReplyLobbyState, GSrv.Serialize<LobbyState>(state), connection, NetDeliveryMethod.ReliableOrdered);
        }

        static public void RequestLobbyStateHandler(NetConnection connection, object data)
        {
            LobbyState state = Lobby.GetLobbyState();
            GSrv.Send(MessageType.ReplyLobbyState, GSrv.Serialize<LobbyState>(state), connection, NetDeliveryMethod.ReliableOrdered);
        }

        static public void EnterRoomHandler(NetConnection connection, object data)
        {
            int roomid = (int)data;
            if(roomid < 0 || Env.RoomNum <= roomid)
            {
                return;
            }

            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            if (!PlayerCaches.InLobby(connection))
            {
                return;
            }

            if (Lobby.GetRoomState(roomid) == 1)
            {
                return;
            }

            Lobby.EnterRoom(connection, roomid);

            GSrv.Send(MessageType.ReplySuccessEnterRoom, connection, NetDeliveryMethod.ReliableOrdered);

            Lobby.SendRoomStateToAll(roomid);
        }

        static public void LeaveRoomHandler(NetConnection connection, object data)
        {
            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            if (PlayerCaches.InLobby(connection))
            {
                return;
            }

            int roomid = PlayerCaches.GetRoomID(connection);

            if(Lobby.GetRoomState(roomid) == 1)
            {
                return;
            }

            Lobby.LeaveRoom(connection, roomid);
            Lobby.SendRoomStateToAll(roomid);

            Lobby.CheckBattleStart(roomid);

            GSrv.Send(MessageType.ReplySuccessLeaveRoom, connection, NetDeliveryMethod.ReliableOrdered);
            LobbyState state = Lobby.GetLobbyState();
            GSrv.Send(MessageType.ReplyLobbyState, GSrv.Serialize<LobbyState>(state), connection, NetDeliveryMethod.ReliableOrdered);
        }

        static public void ChangeFactionHandler(NetConnection connection, object data)
        {
            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            if (PlayerCaches.InLobby(connection))
            {
                return;
            }

            int roomid = PlayerCaches.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 1)
            {
                return;
            }

            Lobby.ChangeFaction(connection, roomid);
            Lobby.SendRoomStateToAll(roomid);
        }

        static public void ChangeReadyHandler(NetConnection connection, object data)
        {
            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            if (PlayerCaches.InLobby(connection))
            {
                return;
            }

            int roomid = PlayerCaches.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 1)
            {
                return;
            }

            Lobby.ChangeReady(connection, roomid);
            Lobby.SendRoomStateToAll(roomid);

            Lobby.CheckBattleStart(roomid);
        }

        static public void TouchCoreHandler(NetConnection connection, object data)
        {
            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            if (PlayerCaches.InLobby(connection))
            {
                return;
            }

            int roomid = PlayerCaches.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 0)
            {
                return;
            }

            Lobby.TouchCore(connection, roomid);
        }

        static public void SetBlockHandler(NetConnection connection, object data)
        {
            BlockData block = null;
            try
            {
                block = GSrv.Deserialize<BlockData>((byte[])data);
            }
            catch
            {
                return;
            }

            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            if (PlayerCaches.InLobby(connection))
            {
                return;
            }

            int roomid = PlayerCaches.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 0)
            {
                return;
            }

            if(!Env.IsInsideWorld(block.x, block.y, block.z))
            {
                return;
            }

            if(Env.IsCorePos(block.x, block.y, block.z))
            {
                return;
            }

            Lobby.ReplySetBlock(roomid, block);
        }

        static public void SendFireHandler(NetConnection connection, object data)
        {
            FireData fire = null;
            try
            {
                fire = GSrv.Deserialize<FireData>((byte[])data);
            }
            catch
            {
                return;
            }

            if (!PlayerCaches.AuthDone(connection))
            {
                return;
            }

            if (PlayerCaches.InLobby(connection))
            {
                return;
            }

            int roomid = PlayerCaches.GetRoomID(connection);

            if (Lobby.GetRoomState(roomid) == 0)
            {
                return;
            }

            Lobby.ReplyFire(connection, roomid, fire);            
        }
    }
}

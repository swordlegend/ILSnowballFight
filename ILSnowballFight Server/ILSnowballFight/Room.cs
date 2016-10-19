using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YasGameLib;

namespace ILSnowballFight
{
    public class Room
    {
        int roomid;
        Dictionary<NetConnection, Player> players = new Dictionary<NetConnection, Player>();
        int roomstate = 0;

        float wonTimer = 0;

        public Room(int roomid)
        {
            this.roomid = roomid;
        }

        public RoomSummary GetRoomSummary()
        {
            return new RoomSummary { roomid = roomid, playernum = players.Count, roomstate = roomstate };
        }

        public void EnterRoom(Player player)
        {
            int blueCount = players.Where(x => x.Value.faction == 0).ToArray().Length;
            int redCount = players.Where(x => x.Value.faction == 1).ToArray().Length;

            if(blueCount > redCount)
            {
                player.faction = 1;
            }
            else
            {
                player.faction = 0;
            }

            player.ready = false;

            players.Add(player.connection, player);
            PlayerCaches.EnterRoom(player.connection, roomid);
        }

        public void SendRoomStateToAll()
        {
            RoomState state = new RoomState();
            state.roomid = roomid;
            foreach(Player player in players.Values)
            {
                state.players.Add(new PlayerState { userid = player.userid, username = player.username, faction = player.faction, ready = player.ready });
            }

            foreach(Player player in players.Values)
            {
                GSrv.Send(MessageType.ReplyRoomState, GSrv.Serialize<RoomState>(state), player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public int GetRoomState()
        {
            return roomstate;
        }

        public Player LeaveRoom(NetConnection connection)
        {
            Player player = players[connection];
            players.Remove(connection);
            return player;
        }

        public void ChangeFaction(NetConnection connection)
        {
            if(players[connection].faction == 0)
            {
                players[connection].faction = 1;
            }
            else
            {
                players[connection].faction = 0;
            }
        }

        public void ChangeReady(NetConnection connection)
        {
            if (players[connection].ready)
            {
                players[connection].ready = false;
            }
            else
            {
                players[connection].ready = true;
            }
        }

        public void CheckBattleStart()
        {
            if (players.Count == 0)
            {
                return;
            }

            Player not_ready_player = players.FirstOrDefault(x => x.Value.ready == false).Value;
            if (not_ready_player != null)
            {
                return;
            }

            PlayerInitDatas datas = new PlayerInitDatas();
            foreach (Player player in players.Values)
            {
                if (player.faction == 0)
                {
                    PlayerSyncData sync = new PlayerSyncData
                    {
                        userid = player.userid,
                        xpos = 16.0f,
                        ypos = 17.0f,
                        zpos = 1.0f,
                        xrot = 0.0f,
                        yrot = 0.0f,
                        animestate = 0,
                        hp = 100
                    };
                    player.sync = sync;
                }
                else
                {
                    PlayerSyncData sync = new PlayerSyncData
                    {
                        userid = player.userid,
                        xpos = 16.0f,
                        ypos = 17.0f,
                        zpos = 63.0f,
                        xrot = 0.0f,
                        yrot = 180.0f,
                        animestate = 0,
                        hp = 100
                    };
                    player.sync = sync;
                }

                player.ResetRespawnTimer();

                datas.datas.Add(new PlayerInitData { username = player.username, faction = player.faction, sync = player.sync });
            }

            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.BattleStart, GSrv.Serialize<PlayerInitDatas>(datas), player.connection, NetDeliveryMethod.ReliableOrdered);
            }

            roomstate = 1;
        }

        public void Push(NetConnection connection, PushData data)
        {
            PlayerSyncData sync = players[connection].sync;
            sync.xpos = data.xpos;
            sync.ypos = data.ypos;
            sync.zpos = data.zpos;
            sync.xrot = data.xrot;
            sync.yrot = data.yrot;
            sync.animestate = data.animestate;
        }

        public void SendSnapShot()
        {
            if(roomstate == 0)
            {
                return;
            }

            PlayerSyncDatas syncs = new PlayerSyncDatas();
            foreach (Player player in players.Values)
            {
                syncs.datas.Add(player.sync);
            }

            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.Snapshot, GSrv.Serialize<PlayerSyncDatas>(syncs), player.connection, NetDeliveryMethod.UnreliableSequenced);
            }
        }

        public void Update(float delta)
        {
            if (roomstate == 0)
            {
                return;
            }

            if(wonTimer > 0)
            {
                wonTimer -= delta;

                if(wonTimer <= 0)
                {
                    BattleEnd();
                }
            }

            foreach (Player player in players.Values)
            {
                player.Update(delta);
            }
        }

        public void TouchCore(NetConnection connection)
        {
            if (wonTimer > 0)
            {
                return;
            }

            wonTimer = 5.0f;

            int faction = players[connection].faction;
            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.ReplyWon, faction, player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        void BattleEnd()
        {
            roomstate = 0;
            foreach (Player player in players.Values)
            {
                player.ready = false;
            }

            RoomState state = new RoomState();
            state.roomid = roomid;
            foreach (Player player in players.Values)
            {
                state.players.Add(new PlayerState { userid = player.userid, username = player.username, faction = player.faction, ready = player.ready });
            }

            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.BattleEnd, player.connection, NetDeliveryMethod.ReliableOrdered);
                GSrv.Send(MessageType.ReplyRoomState, GSrv.Serialize<RoomState>(state), player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void ReplySetBlock(BlockData block)
        {
            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.ReplySetBlock, GSrv.Serialize<BlockData>(block), player.connection, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void ReplyFire(NetConnection connection, FireData fire)
        {
            int userid = players[connection].userid;

            foreach (Player player in players.Values)
            {
                GSrv.Send(MessageType.ReplyFire, userid, player.connection, NetDeliveryMethod.ReliableOrdered);
            }

            if(fire.hit)
            {
                Player player = players.Values.FirstOrDefault(x => x.userid == fire.userid);
                if(player != null)
                {
                    if(fire.parts == 0)
                    {
                        player.Damaged(100);
                    }
                    else if(fire.parts == 1)
                    {
                        player.Damaged(50);
                    }
                    else
                    {
                        player.Damaged(25);
                    }
                }
            }
        }

        public void Disconnect(NetConnection connection)
        {
            Console.WriteLine("Leave:" + players[connection].username);
            players.Remove(connection);

            if (roomstate == 0)
            {
                SendRoomStateToAll();
                CheckBattleStart();
            }
            else
            {
                if(players.Count == 0)
                {
                    roomstate = 0;
                    wonTimer = 0;
                }
            }
        }
    }
}

using Lidgren.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using YasGameLib;

namespace ILSnowballFight
{
    public class NetworkScript : MonoBehaviour
    {
        //UI
        [SerializeField]
        GameObject menu, start, lobby, room;
        [SerializeField]
        Text debugText;

        [SerializeField]
        InputField username;

        string host;
        int port;

        float frameSpan = 0;

        List<GameObject> roomSummaryUIs = new List<GameObject>();
        List<GameObject> roomStateUIs = new List<GameObject>();

        void Start()
        {
            if (!ReadFile(Application.dataPath + "/../Setting/setting.txt"))
            {
                return;
            }

            menu.SetActive(true);
            start.SetActive(false);
            lobby.SetActive(false);
            room.SetActive(false);

            GCli.Init();
            GCli.SetConnectPacketHandler(ConnectHandler);
            GCli.SetDebugPacketHandler(DebugHandler);

            GCli.Connect("ILSnowballFight0.1", host, port);

            World.CreateDefaultBlocks();
        }

        bool ReadFile(string path)
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

                        if (line.StartsWith("Host="))
                        {
                            string sub = line.Substring("Host=".Length);
                            host = sub;
                        }
                        else if (line.StartsWith("Port="))
                        {
                            string sub = line.Substring("Port=".Length);
                            if (!int.TryParse(sub, out port))
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                debugText.text = e.Message;
                return false;
            }

            return true;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            GCli.Receive();

            frameSpan += Time.deltaTime;
            if (frameSpan >= 0.1f)
            {
                frameSpan = 0;

                if(Players.GetPlayer() != null)
                {
                    PushData push = Players.GetPushData();
                    if(push != null)
                    {
                        GCli.Send(MessageType.Push, GCli.Serialize<PushData>(push), NetDeliveryMethod.UnreliableSequenced);
                    }
                }
            }
        }

        void FixedUpdate()
        {
            Players.FixedUpdate(Time.deltaTime);
        }

        void OnDestroy()
        {
            GCli.Shutdown();
        }       

        public void ConnectHandler(NetConnection connection, object data)
        {
            GCli.ClearPacketHandler();
            debugText.text = "";
            start.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyUserID, DataType.Int32, ReplyUserIDHandler);            
        }

        public void SendUserName()
        {
            string name = username.text;
            GCli.Send(MessageType.SendUserName, name, NetDeliveryMethod.ReliableOrdered);
        }

        public void DebugHandler(NetConnection connection, object data)
        {
            string message = (string)data;
            debugText.text = message;
        }

        public void SnapshotHandler(NetConnection connection, object data)
        {
            PlayerSyncDatas datas = GCli.Deserialize<PlayerSyncDatas>((byte[])data);

            Players.UpdatePlayerSyncData(datas.datas);
        }

        public void ReplyUserIDHandler(NetConnection connection, object data)
        {
            Players.userid = (int)data;

            GCli.ClearPacketHandler();
            start.SetActive(false);
            lobby.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyLobbyState, DataType.Bytes, ReplyLobbyStateHandler);
            GCli.SetPacketHandler(MessageType.ReplySuccessEnterRoom, DataType.Null, ReplySuccessEnterRoomHandler);
        }

        public void ReplyLobbyStateHandler(NetConnection connection, object data)
        {
            LobbyState state = GCli.Deserialize<LobbyState>((byte[])data);

            RefreshLobbyUI(state);
        }
        
        void RefreshLobbyUI(LobbyState state)
        {
            //delete
            foreach(GameObject room in roomSummaryUIs)
            {
                Destroy(room);
            }
            roomSummaryUIs.Clear();

            //UI
            for (int i = 0; i < state.summaries.Count; i++)
            {
                GameObject room = GameObject.Instantiate(Resources.Load("RoomSummary"), lobby.transform) as GameObject;
                room.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 142 + -34 * i);
                room.GetComponent<RoomSummaryScript>().Init(state.summaries[i]);
                roomSummaryUIs.Add(room);
            }
        }

        public void RequestLobbyState()
        {
            GCli.Send(MessageType.RequestLobbyState, NetDeliveryMethod.ReliableOrdered);
        }

        public void ReplySuccessEnterRoomHandler(NetConnection connection, object data)
        {
            GCli.ClearPacketHandler();
            lobby.SetActive(false);
            room.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyRoomState, DataType.Bytes, ReplyRoomStateHandler);
            GCli.SetPacketHandler(MessageType.ReplySuccessLeaveRoom, DataType.Null, ReplySuccessLeaveRoomHandler);
            GCli.SetPacketHandler(MessageType.BattleStart, DataType.Bytes, BattleStartHandler);
        }

        public void ReplyRoomStateHandler(NetConnection connection, object data)
        {
            RoomState state = GCli.Deserialize<RoomState>((byte[])data);

            RefreshRoomUI(state);
        }

        void RefreshRoomUI(RoomState state)
        {
            //delete
            foreach (GameObject room in roomStateUIs)
            {
                Destroy(room);
            }
            roomStateUIs.Clear();

            //UI
            int blue = 0;
            int red = 0;
            for (int i = 0; i < state.players.Count; i++)
            {
                GameObject player = GameObject.Instantiate(Resources.Load("PlayerState"), room.transform) as GameObject;
                if(state.players[i].faction == 0)
                {
                    player.GetComponent<RectTransform>().anchoredPosition = new Vector2(-160, 142 + -34 * blue++);
                }
                else
                {
                    player.GetComponent<RectTransform>().anchoredPosition = new Vector2(160, 142 + -34 * red++);
                }
                player.GetComponent<PlayerStateScript>().Init(state.players[i]);
                roomStateUIs.Add(player);
            }
        }

        public void LeaveRoom()
        {
            GCli.Send(MessageType.LeaveRoom, NetDeliveryMethod.ReliableOrdered);
        }

        public void ReplySuccessLeaveRoomHandler(NetConnection connection, object data)
        {
            GCli.ClearPacketHandler();
            room.SetActive(false);
            lobby.SetActive(true);

            GCli.SetPacketHandler(MessageType.ReplyLobbyState, DataType.Bytes, ReplyLobbyStateHandler);
            GCli.SetPacketHandler(MessageType.ReplySuccessEnterRoom, DataType.Null, ReplySuccessEnterRoomHandler);
        }

        public void BattleStartHandler(NetConnection connection, object data)
        {
            PlayerInitDatas inits = GCli.Deserialize<PlayerInitDatas>((byte[])data);

            GCli.ClearPacketHandler();
            room.SetActive(false);
            menu.SetActive(false);

            GCli.SetPacketHandler(MessageType.Snapshot, DataType.Bytes, SnapshotHandler);
            GCli.SetPacketHandler(MessageType.ReplyWon, DataType.Int32, ReplyWonHandler);
            GCli.SetPacketHandler(MessageType.BattleEnd, DataType.Null, BattleEndHandler);
            GCli.SetPacketHandler(MessageType.ReplySetBlock, DataType.Bytes, ReplySetBlockHandler);
            GCli.SetPacketHandler(MessageType.ReplyFire, DataType.Int32, ReplyFire);

            foreach (PlayerInitData init in inits.datas)
            {
                if(init.sync.userid == Players.userid)
                {
                    World.SetCoreFaction(init.faction);

                    Players.AddPlayer(init);
                }
            }
            foreach (PlayerInitData init in inits.datas)
            {
                if (init.sync.userid != Players.userid)
                {
                    Players.AddOtherPlayer(init);
                }
            }
        }

        public void ReplyWonHandler(NetConnection connection, object data)
        {
            int faction = (int)data;

            GameObject won = Instantiate(Resources.Load("WonText"), GameObject.Find("Canvas").transform) as GameObject;
            won.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            won.GetComponent<WonTextScript>().Init(faction);
        }

        public void BattleEndHandler(NetConnection connection, object data)
        {
            GCli.ClearPacketHandler();

            Players.Destroy();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            World.CreateDefaultBlocks();

            menu.SetActive(true);
            room.SetActive(true);            

            GCli.SetPacketHandler(MessageType.ReplyRoomState, DataType.Bytes, ReplyRoomStateHandler);
            GCli.SetPacketHandler(MessageType.ReplySuccessLeaveRoom, DataType.Null, ReplySuccessLeaveRoomHandler);
            GCli.SetPacketHandler(MessageType.BattleStart, DataType.Bytes, BattleStartHandler);
        }

        public void ReplySetBlockHandler(NetConnection connection, object data)
        {
            BlockData block = GCli.Deserialize<BlockData>((byte[])data);

            World.SetBlock(block.x, block.y, block.z, block.blocktype);
        }

        public void ReplyFire(NetConnection connection, object data)
        {
            int userid = (int)data;

            Players.ReplyFire(userid);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YasGameLib
{
    public enum MessageType
    {
        Connect,
        Disconnect,
        Debug,
        Login,
        LoginSuccess,
        LoginFailed,
        Register,
        RegisterSuccess,
        RegisterFailed,
        Join,
        Snapshot,
        Push,
        SendMessage,
        BroadcastMessage,
        SendUserName,
        ReplyUserID,
        ReplyLobbyState,
        RequestLobbyState,
        EnterRoom,
        ReplySuccessEnterRoom,
        ReplyRoomState,
        LeaveRoom,
        ReplySuccessLeaveRoom,
        ChangeFaction,
        ChangeReady,
        BattleStart,
        TouchCore,
        ReplyWon,
        BattleEnd,
        SetBlock,
        ReplySetBlock,
        SendFire,
        ReplyFire,
    }
}

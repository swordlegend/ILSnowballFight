using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YasGameLib;

namespace ILSnowballFight
{
    class RoomSummaryScript : MonoBehaviour
    {
        [SerializeField]
        Text roomName, roomState, playerNum;
        [SerializeField]
        Image roomStateImage;
        [SerializeField]
        GameObject enterButton;

        RoomSummary summary;

        public void Init(RoomSummary summary)
        {
            this.summary = summary;

            roomName.text = "Room" + summary.roomid.ToString();
            if(summary.roomstate == 0)
            {
                roomState.text = "Preparation";
                roomStateImage.color = Color.green;
            }
            else
            {
                roomState.text = "In Battle";
                roomStateImage.color = Color.red;
                enterButton.SetActive(false);
            }
            playerNum.text = summary.playernum.ToString();
        }

        public void EnterRoom()
        {
            GCli.Send(MessageType.EnterRoom, summary.roomid, NetDeliveryMethod.ReliableOrdered);
        }
    }
}

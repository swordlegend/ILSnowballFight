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
    public class PlayerStateScript : MonoBehaviour
    {
        [SerializeField]
        Text userName;
        [SerializeField]
        Image factionImage;
        [SerializeField]
        GameObject changeButton, readyButton, readyIcon, cancelIcon, ready;

        PlayerState state;

        public void Init(PlayerState state)
        {
            this.state = state;

            if(state.faction == 0)
            {
                factionImage.color = Color.blue;
            }
            else
            {
                factionImage.color = Color.red;
            }

            userName.text = state.username;

            if(!state.ready)
            {
                ready.SetActive(false);
            }

            if (state.userid == Players.userid)
            {
                if (state.ready)
                {
                    readyIcon.SetActive(false);
                    changeButton.SetActive(false);
                }
                else
                {
                    cancelIcon.SetActive(false);
                }
            }
            else
            {
                changeButton.SetActive(false);
                readyButton.SetActive(false);
            }
        }

        public void ChangeFaction()
        {
            GCli.Send(MessageType.ChangeFaction, NetDeliveryMethod.ReliableOrdered);
        }

        public void ChangeReady()
        {
            GCli.Send(MessageType.ChangeReady, NetDeliveryMethod.ReliableOrdered);
        }
    }
}

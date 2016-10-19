using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YasGameLib;

namespace ILSnowballFight
{
    class CoreScript : MonoBehaviour
    {
        bool isEnemyCore;
        public void Init(bool isEnemyCore)
        {
            this.isEnemyCore = isEnemyCore;
        }

        void Update()
        {
            transform.Rotate(new Vector3(0, 90.0f * Time.deltaTime, 0), Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if(isEnemyCore)
            {
                if(other.tag == "MyPlayer")
                {
                    GCli.Send(MessageType.TouchCore, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }
    }
}

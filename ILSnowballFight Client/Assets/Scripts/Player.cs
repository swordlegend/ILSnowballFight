using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ILSnowballFight
{
    public class Player : PlayerBase
    {
        PrefabManager prefab, hp;

        public Player(PlayerInitData init) : base(init)
        {
            prefab = new PrefabManager();
            prefab.LoadPrefab("Player");
            prefab.GetInstance().GetComponent<PlayerPrefabScript>().Init(init);

            hp = new PrefabManager();
            hp.LoadPrefab("HPText", GameObject.Find("Canvas").transform);
            hp.GetInstance().GetComponent<HPTextScript>().Init(init);
        }

        public PushData GetPushData()
        {
            if (init.sync.hp != 0)
            {
                return prefab.GetInstance().GetComponent<PlayerPrefabScript>().GetPushData();
            }
            else
            {
                return null;
            }
        }

        public int GetFaction()
        {
            return init.faction;
        }

        public Vector3 GetPosition()
        {
            return prefab.GetInstance().transform.position;
        }

        public Vector3 GetForward()
        {
            return prefab.GetInstance().transform.forward;
        }

        public override void ManageDiff()
        {
            if (init.sync.hp == 0 && previous.hp != 0)
            {
                prefab.ReloadPrefab("PlayerDead");
                prefab.GetInstance().transform.position = new Vector3(init.sync.xpos, init.sync.ypos, init.sync.zpos);
                prefab.GetInstance().transform.localEulerAngles = new Vector3(0, init.sync.yrot, 0);
            }
            else if (init.sync.hp != 0 && previous.hp == 0)
            {
                prefab.ReloadPrefab("Player");
                prefab.GetInstance().GetComponent<PlayerPrefabScript>().Init(init);
            }
            else if(init.sync.hp < previous.hp && init.sync.hp != 0)
            {
                GameObject.Find("Hit").GetComponent<HitScript>().Hit();
            }
        }

        public void Destroy()
        {
            prefab.Destroy();
            hp.Destroy();
        }
    }
}

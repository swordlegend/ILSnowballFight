using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ILSnowballFight
{
    class HPTextScript : MonoBehaviour
    {
        [SerializeField]
        Text hpText;

        PlayerInitData init;

        public void Init(PlayerInitData init)
        {
            this.init = init;
        }

        void Update()
        {
            hpText.text = init.sync.hp.ToString();
        }
    }
}

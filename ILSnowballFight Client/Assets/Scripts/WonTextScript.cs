using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ILSnowballFight
{
    class WonTextScript : MonoBehaviour
    {
        [SerializeField]
        Text wonText;

        public void Init(int faction)
        {
            if(faction == 0)
            {
                wonText.text = "<color=blue>Blue</color> Team Won";
            }
            else
            {
                wonText.text = "<color=red>Red</color> Team Won";
            }

            Destroy(gameObject, 4.5f);
        }
    }
}

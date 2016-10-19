using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ILSnowballFight
{
    public class DeadScript : MonoBehaviour
    {
        void Start()
        {
            Destroy(gameObject, 3.0f);
        }
    }

}

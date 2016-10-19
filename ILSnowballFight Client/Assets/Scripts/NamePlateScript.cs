using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ILSnowballFight
{
    public class NamePlateScript : MonoBehaviour
    {
        Transform parent;
        float offset = 0.9f;

        public void Init(Transform parent, string name)
        {
            this.parent = parent;

            GetComponent<Text>().text = name;

            SetPosition();
        }
        // Update is called once per frame
        void Update()
        {
            SetPosition();
        }

        void SetPosition()
        {
            Vector3 worldPos = parent.position;
            worldPos.y += offset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            transform.position = screenPos;
        }
    }
}

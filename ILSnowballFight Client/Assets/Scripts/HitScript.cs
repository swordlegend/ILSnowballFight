using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ILSnowballFight
{
    class HitScript : MonoBehaviour
    {
        [SerializeField]
        Image image;
        [SerializeField]
        AudioSource source;

        public void Hit()
        {
            source.Stop();
            source.Play();

            image.color = new Color(1.0f, 0, 0, 0.5f);
        }

        void Update()
        {
            image.color = Color.Lerp(image.color, Color.clear, 10.0f * Time.deltaTime);
        }
    }
}

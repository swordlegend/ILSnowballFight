using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ILSnowballFight
{
    class OtherPlayerPrefabScript : MonoBehaviour
    {
        [SerializeField]
        Transform model, spine, rightHand;

        GameObject gun_or_box;

        float previous_xrot;
        float latest_xrot;

        Animator anime;
        int animeState;

        //fireSound
        AudioSource fireSound;
        //muzzleFlash
        EllipsoidParticleEmitter muzzleFlash;
        float particleTimer = 0.0f;

        public PlayerInitData init { get; private set; }

        /*void Start()
        {
            anime = GetComponentInChildren<Animator>();

            animeState = 1;
            SetAnime();

            previous_xrot = 0;
            latest_xrot = 0;
        }*/

        public void Init(PlayerInitData init)
        {
            this.init = init;

            anime = GetComponentInChildren<Animator>();

            transform.position = new Vector3(init.sync.xpos, init.sync.ypos, init.sync.zpos);
            previous_xrot = init.sync.xrot;
            latest_xrot = init.sync.xrot;
            transform.localEulerAngles = new Vector3(0, init.sync.yrot, 0);
            this.animeState = init.sync.animestate;

            SetAnime();
        }

        void SetAnime()
        {
            if(gun_or_box != null)
            {
                Destroy(gun_or_box);
            }

            if (animeState == 0)
            {
                anime.SetTrigger("GunIdle");
                gun_or_box = Instantiate(Resources.Load("M4A1"), rightHand) as GameObject;
                gun_or_box.transform.localPosition = new Vector3(0.1545196f, -0.04906856f, -0.1303249f);
                gun_or_box.transform.localEulerAngles = new Vector3(0.855f, 313.999f, -77.73701f);

                model.localEulerAngles = new Vector3(0, 22.756f, 0);

                fireSound = gun_or_box.GetComponent<AudioSource>();
                muzzleFlash = gun_or_box.GetComponentInChildren<EllipsoidParticleEmitter>();
            }
            else if (animeState == 1)
            {
                anime.SetTrigger("GunWalk");
                gun_or_box = Instantiate(Resources.Load("M4A1"), rightHand) as GameObject;
                gun_or_box.transform.localPosition = new Vector3(0.1274473f, -0.05936916f, -0.1628172f);
                gun_or_box.transform.localEulerAngles = new Vector3(-0.956f, 328.471f, -86.02901f);

                model.localEulerAngles = Vector3.zero;

                fireSound = gun_or_box.GetComponent<AudioSource>();
                muzzleFlash = gun_or_box.GetComponentInChildren<EllipsoidParticleEmitter>();
            }
            else if (animeState == 2)
            {
                anime.SetTrigger("BoxIdle");
                gun_or_box = Instantiate(Resources.Load("Box"), rightHand) as GameObject;
                gun_or_box.transform.localPosition = new Vector3(-0.009517208f, -0.185615f, 0.1949636f);
                gun_or_box.transform.localEulerAngles = new Vector3(12.54f, 48.13f, 67.928f);

                model.localEulerAngles = Vector3.zero;

                if(init.faction ==0)
                {
                    gun_or_box.GetComponent<Renderer>().material.color = Color.cyan;
                }
                else
                {
                    gun_or_box.GetComponent<Renderer>().material.color = Color.magenta;
                }
            }
            else if(animeState == 3)
            {
                anime.SetTrigger("BoxWalk");
                gun_or_box = Instantiate(Resources.Load("Box"), rightHand) as GameObject;
                gun_or_box.transform.localPosition = new Vector3(-0.009517208f, -0.185615f, 0.1949636f);
                gun_or_box.transform.localEulerAngles = new Vector3(12.54f, 48.13f, 67.928f);

                model.localEulerAngles = Vector3.zero;

                if (init.faction == 0)
                {
                    gun_or_box.GetComponent<Renderer>().material.color = Color.cyan;
                }
                else
                {
                    gun_or_box.GetComponent<Renderer>().material.color = Color.magenta;
                }
            }
        }

        public void Fire()
        {
            if(animeState == 0 || animeState == 1)
            {
                fireSound.Stop();
                fireSound.Play();

                muzzleFlash.emit = false;
                muzzleFlash.emit = true;
                particleTimer = 0.0f;
            }
        }

        void Update()
        {
            /*if(Input.GetMouseButtonDown(0))
            {
                animeState++;
                if(animeState == 4)
                {
                    animeState = 0;
                }
                SetAnime();
            }
            if (Input.GetMouseButtonDown(1))
            {
                Fire();
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                latest_xrot += 10.0f;
            }*/

            if (animeState == 0 || animeState == 1)
            {
                if (muzzleFlash.emit)
                {
                    particleTimer += Time.deltaTime;
                    if (particleTimer >= 0.1f)
                    {
                        muzzleFlash.emit = false;
                    }
                }
            }
        }

        public void Interpolation(PlayerSyncData latest, float delta)
        {
            Vector3 latestPos = new Vector3(latest.xpos, latest.ypos, latest.zpos);
            transform.position = Vector3.Lerp(transform.position, latestPos, delta * 10.0f);

            transform.localEulerAngles = new Vector3(0, Mathf.LerpAngle(transform.localEulerAngles.y, latest.yrot, delta * 10.0f), 0);
            latest_xrot = latest.xrot;

            if (this.animeState != latest.animestate)
            {
                this.animeState = latest.animestate;
                SetAnime();
            }
        }

        void LateUpdate()
        {
            previous_xrot = Mathf.LerpAngle(previous_xrot, latest_xrot, Time.deltaTime * 10.0f);

            Quaternion memory = transform.localRotation;
            transform.rotation = Quaternion.identity;
            spine.Rotate(new Vector3(previous_xrot, 0, 0), Space.World);
            transform.localRotation = memory;
        }
    }
}

using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using YasGameLib;

namespace ILSnowballFight
{
    class PlayerPrefabScript : MonoBehaviour
    {
        [SerializeField]
        FirstPersonController fpsCon;
        [SerializeField]
        Transform fpsCamera;
        [SerializeField]
        GameObject gun, box;

        bool haveGun;
        float weaponTimer = 1.0f;
        float fireTimer = 0.0f;

        //fireSound
        [SerializeField]
        AudioSource fireSound;
        //fireAnime
        [SerializeField]
        Animator fireAnime;
        //muzzleFlash
        [SerializeField]
        EllipsoidParticleEmitter muzzleFlash;
        float particleTimer = 0.0f;

        GameObject dot;

        const float distance = 4.0f;

        PlayerInitData init;

        void Start()
        {
            //haveGun = true;
            //weaponTimer = 1.0f;

            dot = Instantiate(Resources.Load("Dot"), GameObject.Find("Canvas").transform) as GameObject;
            dot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        void OnDestroy()
        {
            Destroy(dot);
        }

        public void Init(PlayerInitData init)
        {
            this.init = init;

            transform.position = new Vector3(init.sync.xpos, init.sync.ypos, init.sync.zpos);
            transform.localEulerAngles = new Vector3(0, init.sync.yrot, 0);

            if(init.sync.animestate == 0 || init.sync.animestate == 1)
            {
                haveGun = true;
            }
            else
            {
                haveGun = false;
            }

            if(init.faction == 0)
            {
                box.GetComponent<Renderer>().material.color = Color.cyan;
            }
            else
            {
                box.GetComponent<Renderer>().material.color = Color.magenta;
            }
        }

        public PushData GetPushData()
        {
            float xpos = transform.position.x;
            float ypos = transform.position.y;
            float zpos = transform.position.z;
            float xrot = fpsCamera.localEulerAngles.x;
            float yrot = transform.localEulerAngles.y;
            int animestate;
            if(haveGun)
            {
                animestate = fpsCon.isMoving ? 1 : 0;
            }
            else
            {
                animestate = fpsCon.isMoving ? 3 : 2;
            }

            PushData data = new PushData();
            data.xpos = xpos;
            data.ypos = ypos;
            data.zpos = zpos;
            data.xrot = xrot;
            data.yrot = yrot;
            data.animestate = animestate;

            return data;
        }

        void Update()
        {
            WeaponSelect();

            Shoot();

            Construct();

            if (muzzleFlash.emit)
            {
                particleTimer += Time.deltaTime;
                if (particleTimer >= 0.1f)
                {
                    muzzleFlash.emit = false;
                }
            }
        }

        void WeaponSelect()
        {
            if (weaponTimer > 0)
            {
                weaponTimer -= Time.deltaTime;
                if (weaponTimer <= 0)
                {
                    if (haveGun)
                    {
                        gun.SetActive(true);
                    }
                    else
                    {
                        box.SetActive(true);
                    }
                }
            }

            float wheel = Input.GetAxis("Mouse ScrollWheel");
            if (wheel != 0 && weaponTimer <= 0)
            {
                weaponTimer = 1.0f;

                haveGun = !haveGun;
                gun.SetActive(false);
                box.SetActive(false);
            }
        }

        void Shoot()
        {
            if (fireTimer > 0)
            {
                fireTimer -= Time.deltaTime;
            }

            if (weaponTimer <= 0 && haveGun)
            {
                if (Input.GetButton("Fire1"))
                {
                    if (fireTimer <= 0)
                    {
                        fireTimer = 0.25f;
                        Fire();
                    }
                }
            }
        }

        void Construct()
        {
            if (weaponTimer <= 0 && !haveGun)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    RaycastHit hit;

                    if (Physics.Raycast(fpsCamera.position, fpsCamera.forward, out hit, distance))
                    {
                        Vector3 hitPoint = hit.point + hit.normal * 0.1f;
                        int x = (int)hitPoint.x;
                        int y = (int)hitPoint.y;
                        int z = (int)hitPoint.z;

                        Collider[] cols = Physics.OverlapBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), new Vector3(0.49f, 0.49f, 0.49f));
                        if (cols.Length == 0)
                        {
                            if (Env.IsInsideWorld(x, y, z) && !Env.IsCorePos(x, y, z))
                            {
                                BlockData block = new BlockData { x = x, y = y, z = z, blocktype = init.faction == 0 ? 2 : 3 };
                                GCli.Send(MessageType.SetBlock, GCli.Serialize<BlockData>(block), NetDeliveryMethod.ReliableOrdered);
                                //World.SetBlock(x, y, z, init.faction == 0 ? 2 : 3);
                            }
                        }
                    }
                }
                else if (Input.GetButtonDown("Fire2"))
                {
                    RaycastHit hit;

                    if (Physics.Raycast(fpsCamera.position, fpsCamera.forward, out hit, distance))
                    {
                        Vector3 hitPoint = hit.point - hit.normal * 0.1f;
                        int x = (int)hitPoint.x;
                        int y = (int)hitPoint.y;
                        int z = (int)hitPoint.z;

                        if (Env.IsInsideWorld(x, y, z) && !Env.IsCorePos(x, y, z))
                        {
                            BlockData block = new BlockData { x = x, y = y, z = z, blocktype = 0 };
                            GCli.Send(MessageType.SetBlock, GCli.Serialize<BlockData>(block), NetDeliveryMethod.ReliableOrdered);
                            //World.SetBlock(x, y, z, 0);
                        }
                    }
                }
            }
        }

        void Fire()
        {
            fireSound.Stop();
            fireSound.Play();

            fireAnime.SetTrigger("Fire");

            muzzleFlash.emit = false;
            muzzleFlash.emit = true;
            particleTimer = 0.0f;

            RaycastHit hit;

            if (Physics.Raycast(fpsCamera.position, fpsCamera.forward, out hit, 100f))
            {
                string tag = hit.transform.tag;
                if (tag == "Head")
                {
                    CollisionDetection(hit.transform, 0);
                }
                else if (tag == "Body")
                {
                    CollisionDetection(hit.transform, 1);
                }
                else if (tag == "Hand")
                {
                    CollisionDetection(hit.transform, 2);
                }
                else if (tag == "Leg")
                {
                    CollisionDetection(hit.transform, 3);
                }
                else
                {
                    SendFire(false, 0, 0);
                }
            }
            else
            {
                SendFire(false, 0, 0);
            }
        }

        void CollisionDetection(Transform hitTransform, int parts)
        {
            OtherPlayerPrefabScript other = GetOtherPlayerPrefabScript(hitTransform);

            if(other.init.faction != init.faction)
            {
                SendFire(true, parts, other.init.sync.userid);
            }
            else
            {
                SendFire(false, parts, other.init.sync.userid);
            }
        }

        void SendFire(bool hit, int parts, int userid)
        {
            FireData data = new FireData {hit = hit, parts = parts, userid = userid};

            GCli.Send(MessageType.SendFire, GCli.Serialize<FireData>(data), NetDeliveryMethod.ReliableOrdered);
        }

        OtherPlayerPrefabScript GetOtherPlayerPrefabScript(Transform hit)
        {
            GameObject tar = hit.gameObject;
            while (true)
            {
                if (tar.tag == "Player")
                {
                    break;
                }
                else
                {
                    tar = tar.transform.parent.gameObject;
                }
            }
            return tar.GetComponent<OtherPlayerPrefabScript>();
        }
    }
}

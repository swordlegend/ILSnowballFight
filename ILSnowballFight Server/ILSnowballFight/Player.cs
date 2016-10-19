using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace ILSnowballFight
{
    public class Player
    {
        static int userCount = 0;
        public int userid { get; private set; }
        public string username { get; private set; }
        public NetConnection connection { get; private set; }

        //room
        public int faction;
        public bool ready;
        public PlayerSyncData sync;

        float respawnTimer;

        public Player(NetConnection connection, string username)
        {
            userid = userCount++;
            this.username = username;
            this.connection = connection;
        }

        public void ResetRespawnTimer()
        {
            respawnTimer = 0;
        }
        
        public void Update(float delta)
        {
            if(respawnTimer > 0)
            {
                respawnTimer -= delta;
                if(respawnTimer <= 0)
                {
                    sync.hp = 100;
                    if(faction == 0)
                    {
                        sync.xpos = 16.0f;
                        sync.ypos = 17.0f;
                        sync.zpos = 1.0f;
                        sync.xrot = 0.0f;
                        sync.yrot = 0.0f;
                        sync.animestate = 0;
                    }
                    else
                    {
                        sync.xpos = 16.0f;
                        sync.ypos = 17.0f;
                        sync.zpos = 63.0f;
                        sync.xrot = 0.0f;
                        sync.yrot = 180.0f;
                        sync.animestate = 0;
                    }
                }
            }
        }

        public void Damaged(int amount)
        {
            if(sync.hp == 0)
            {
                return;
            }

            sync.hp -= amount;

            if(sync.hp <= 0)
            {
                sync.hp = 0;
                respawnTimer = 10.0f;
            }
        }
    }
}

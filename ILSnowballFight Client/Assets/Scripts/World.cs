using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YasGameLib;
using UnityEngine;

namespace ILSnowballFight
{
    public static class World
    {
        static int[,,] blocks = null;
        static Chunk[] chunks;

        public static void CreateDefaultBlocks()
        {
            if(blocks != null)
            {
                foreach(Chunk chunk in chunks)
                {
                    chunk.Destroy();
                }
            }

            blocks = Env.CreateDefaultBlocks();

            chunks = new Chunk[Env.XBlockN];
            for(int x=0; x<Env.XBlockN; x++)
            {
                chunks[x] = new Chunk();
                chunks[x].CreateDefaultBlocks(x, blocks);
            }            
        }

        public static void SetCoreFaction(int faction)
        {
            if(faction == 0)
            {
                GameObject.Find("BlueCore").GetComponent<CoreScript>().Init(false);
                GameObject.Find("RedCore").GetComponent<CoreScript>().Init(true);
            }
            else
            {
                GameObject.Find("BlueCore").GetComponent<CoreScript>().Init(true);
                GameObject.Find("RedCore").GetComponent<CoreScript>().Init(false);
            }
        }

        public static void SetBlock(int x, int y, int z, int blocktype)
        {
            blocks[x, y, z] = blocktype;
            for(int i=x-1; i<=x+1; i++)
            {
                if(i < 0 || i >= Env.XBlockN)
                {
                    continue;
                }
                else
                {
                    chunks[i].Destroy();
                    chunks[i].CreateDefaultBlocks(i, blocks);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YasGameLib;

namespace ILSnowballFight
{
    public class Chunk
    {
        Dictionary<int, GameObject> prefabs = new Dictionary<int, GameObject>();

        public void CreateDefaultBlocks(int x, int[,,] blocks)
        {
            for (int z = 0; z < Env.ZBlockN; z++)
            {
                for (int y = 0; y < Env.YBlockN; y++)
                {
                    if (blocks[x, y, z] != 0 &&
                        (!RightIsBlock(blocks, x, y, z) || !LeftIsBlock(blocks, x, y, z) ||
                        !UpIsBlock(blocks, x, y, z) || !DownIsBlock(blocks, x, y, z) ||
                        !ForwardIsBlock(blocks, x, y, z) || !BackIsBlock(blocks, x, y, z)))
                    {
                        if (!prefabs.ContainsKey(blocks[x, y, z]))
                        {
                            GameObject chunk = GameObject.Instantiate(Resources.Load("Chunk")) as GameObject;
                            prefabs.Add(blocks[x, y, z], chunk);

                        }
                        GameObject parent = prefabs[blocks[x, y, z]];

                        GameObject cube = GameObject.Instantiate(Resources.Load("Cube"), parent.transform) as GameObject;
                        cube.transform.position = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                    }
                }
            }

            foreach (KeyValuePair<int, GameObject> chunk in prefabs)
            {
                chunk.Value.GetComponent<Combine>().CombineCubes();
                chunk.Value.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));

                if(chunk.Key == 2)
                {
                    chunk.Value.GetComponent<Renderer>().material.color = Color.cyan;
                }
                else if(chunk.Key == 3)
                {
                    chunk.Value.GetComponent<Renderer>().material.color = Color.magenta;
                }
            }
        }

        public void Destroy()
        {
            foreach(GameObject prefab in prefabs.Values)
            {
                GameObject.Destroy(prefab);
            }
            prefabs.Clear();
        }

        bool RightIsBlock(int[,,] blocks, int x, int y, int z)
        {
            if (x == Env.XBlockN - 1)
            {
                return false;
            }
            else
            {
                return blocks[x + 1, y, z] != 0;
            }
        }

        bool LeftIsBlock(int[,,] blocks, int x, int y, int z)
        {
            if (x == 0)
            {
                return false;
            }
            else
            {
                return blocks[x - 1, y, z] != 0;
            }
        }

        bool UpIsBlock(int[,,] blocks, int x, int y, int z)
        {
            if (y == Env.YBlockN - 1)
            {
                return false;
            }
            else
            {
                return blocks[x, y + 1, z] != 0;
            }
        }

        bool DownIsBlock(int[,,] blocks, int x, int y, int z)
        {
            if (y == 0)
            {
                return true;
            }
            else
            {
                return blocks[x, y - 1, z] != 0;
            }
        }

        bool ForwardIsBlock(int[,,] blocks, int x, int y, int z)
        {
            if (z == Env.ZBlockN - 1)
            {
                return false;
            }
            else
            {
                return blocks[x, y, z + 1] != 0;
            }
        }

        bool BackIsBlock(int[,,] blocks, int x, int y, int z)
        {
            if (z == 0)
            {
                return false;
            }
            else
            {
                return blocks[x, y, z - 1] != 0;
            }
        }
    }
}

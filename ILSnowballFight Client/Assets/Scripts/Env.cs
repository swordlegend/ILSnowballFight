using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YasGameLib
{
    public static class Env
    {
        public const int XBlockN = 32;
        public const int YBlockN = 32;
        public const int ZBlockN = 64;

        public const int RoomNum = 10;

        public static int[,,] CreateDefaultBlocks()
        {
            int[,,] blocks = new int[XBlockN, YBlockN, ZBlockN];

            for(int x=0; x<XBlockN; x++)
            {
                for (int z = 0; z < ZBlockN; z++)
                {
                    for (int y = 0; y < YBlockN; y++)
                    {
                        if(y < 16)
                        {
                            blocks[x, y, z] = 1;
                        }
                        else
                        {
                            blocks[x, y, z] = 0;
                        }
                    }
                }
            }

            return blocks;
        }

        public static bool IsInsideWorld(int x, int y, int z)
        {
            if (0 <= x && x < XBlockN &&
                0 <= y && y < YBlockN &&
                0 <= z && z < ZBlockN)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsCorePos(int x, int y, int z)
        {
            if((x == 15 || x == 16) && (y == 16 || y == 17) && (z == 0 || z == 1 || z == 62 || z == 63))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

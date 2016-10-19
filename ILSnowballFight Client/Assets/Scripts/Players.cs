using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILSnowballFight
{
    public static class Players
    {
        public static int userid;

        static Player player;
        static Dictionary<int, OtherPlayer> otherPlayers = new Dictionary<int, OtherPlayer>();

        public static void AddPlayer(PlayerInitData init)
        {
            player = new Player(init);
        }

        public static void AddOtherPlayer(PlayerInitData init)
        {
            otherPlayers.Add(init.sync.userid, new OtherPlayer(init));
        }

        public static Player GetPlayer()
        {
            return player;
        }        

        public static PushData GetPushData()
        {
            return player.GetPushData();
        }

        public static void UpdatePlayerSyncData(List<PlayerSyncData> syncs)
        {
            foreach (OtherPlayer other in otherPlayers.Values)
            {
                other.ResetUpdateFlag();
            }

            foreach (PlayerSyncData sync in syncs)
            {
                if (sync.userid == userid)
                {
                    player.ReceiveLatestData(sync);
                }
                else if (otherPlayers.ContainsKey(sync.userid))
                {
                    otherPlayers[sync.userid].ReceiveLatestData(sync);
                }
                else
                {
                    
                }
            }

            var removes = otherPlayers.Where(f => f.Value.updated == false).ToArray();
            foreach (var remove in removes)
            {
                remove.Value.Destroy();
                otherPlayers.Remove(remove.Key);
            }
        }

        public static void FixedUpdate(float delta)
        {
            foreach (OtherPlayer other in otherPlayers.Values)
            {
                other.Update(delta);
            }
        }

        public static void Destroy()
        {
            player.Destroy();
            player = null;

            foreach (OtherPlayer other in otherPlayers.Values)
            {
                other.Destroy();
            }
            otherPlayers.Clear();
        }

        public static void ReplyFire(int userid)
        {
            KeyValuePair<int, OtherPlayer> other = otherPlayers.FirstOrDefault(x => x.Value.GetUserID() == userid);

            if(!other.Equals(default(KeyValuePair<int, OtherPlayer>)))
            {
                other.Value.ReplyFire();
            }
        }
    }
}

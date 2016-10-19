using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILSnowballFight
{
    public abstract class PlayerBase
    {
        protected PlayerInitData init;
        protected PlayerSyncData previous;
        public bool updated = true;

        public PlayerBase(PlayerInitData init)
        {
            this.init = init;
            previous = init.sync;
        }

        public void ReceiveLatestData(PlayerSyncData sync)
        {
            updated = true;

            previous = init.sync;
            init.sync = sync;

            ManageDiff();
        }

        public void ResetUpdateFlag()
        {
            updated = false;
        }

        public abstract void ManageDiff();
    }
}

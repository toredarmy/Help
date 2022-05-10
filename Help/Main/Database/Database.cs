using System;
using System.Collections.Generic;

namespace Help.Main.Database
{
    internal sealed partial class Database : Base
    {
        public event Action<DateTime> LastEvent;
        public event Action<List<Alarm>> AlarmsEvent;

        public void Init()
        {
            if (Settings.IsServiceRunning(Settings.ServerService))
            {
#if DEBUG
                Delete();
#endif
                if (CreateDatabase())
                {
                    if (CreateTable())
                    {
                    }
                }
            }
        }
    }
}

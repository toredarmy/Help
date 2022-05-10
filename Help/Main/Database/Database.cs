using System;

namespace Help.Main.Database
{
    internal sealed partial class Database : Base
    {
        public bool IsInited { get; private set; }

        public void Init()
        {
#if DEBUG
            Delete();
#endif
            if (CreateDatabase())
            {
                if (CreateTable())
                {
                    IsInited = true;
                }
            }
        }
    }
}

namespace Help.Main.Database
{
    internal sealed partial class Database : Base
    {
        public bool IsInited { get; private set; }

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
                        IsInited = true;
                    }
                }
            }
        }
    }
}

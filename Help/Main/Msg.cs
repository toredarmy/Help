using System;

namespace Help.Main
{
    [Serializable]
    internal struct Msg
    {
        public string To { get; set; }
        public string From { get; set; }
        public object Data { get; set; }
        public string DataType { get; set; }
    }
}

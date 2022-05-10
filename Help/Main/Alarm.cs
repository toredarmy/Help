using System;
using System.Data.SqlClient;

using Help.Main.Extension;

namespace Help.Main
{
    [Serializable]
    internal sealed class Alarm
    {
        public int Id { get; set; }
        public Guid Guid { get; }
        public DateTime Time { get; }
        public string Database { get; }
        public string Object { get; }
        public string Address { get; }
        public string AddressName { get; }
        public int Section { get; }
        public string SectionName { get; }
        public string Event { get; }
        public int Send { get; set; }

        /// <summary>
        /// Инициализация тревоги из собственной базы данных
        /// </summary>
        public Alarm(SqlDataReader reader)
        {
            Id = reader.GetValue<int>("Id");
            Guid = reader.GetValue<Guid>("GUID");
            Time = reader.GetValue<DateTime>("Time");
            Database = reader.GetValue<string>("Src");
            Object = reader.GetValue<string>("Obj");
            Address = reader.GetValue<string>("Address");
            AddressName = reader.GetValue<string>("AddressName");
            Section = reader.GetValue<int>("Section");
            SectionName = reader.GetValue<string>("SectionName");
            Event = reader.GetValue<string>("Event");
            Send = reader.GetValue<int>("Send");
        }

        /// <summary>
        /// Инициализация тревоги из базы данных Орион Про
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="src">Имя базы данных Орион Про</param>
        /// <param name="obj">Имя обьекта</param>
        public Alarm(SqlDataReader reader, string src, string obj)
        {
            Guid = reader.GetValue<Guid>("GUID");
            Time = reader.GetValue<DateTime>("Time");
            Database = src;
            Object = obj;
            Address = reader.GetValue<string>("Address");
            AddressName = reader.GetValue<string>("AddressName");
            Section = reader.GetValue<int>("Section");
            SectionName = reader.GetValue<string>("SectionName");
            Event = reader.GetValue<string>("Event");
        }
    }
}

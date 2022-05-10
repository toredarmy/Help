using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Help.Main.Orion
{
    internal sealed class Orion : Base
    {
        public event Action GetLastEvent;
        public event Action<List<Alarm>> AlarmsEvent;

        private DateTime? _last;
        private bool _lastRequest;
        private TimeSpan _lastDatabase;
        private TimeSpan _lastRegistry;
        private bool _isCorrectDatabase = true;

        public void Start()
        {
            Task.Run(async () =>
            {
                Log("Start");

                var timer = new Stopwatch();
                timer.Start();

                _lastDatabase = timer.Elapsed;
                _lastRegistry = timer.Elapsed;

                while (true)
                {
                    if (_last == null && !_lastRequest)
                    {
                        _lastRequest = true;
                        GetLastEvent?.Invoke();
                    }

                    // check database every 5 seconds
                    if ((timer.Elapsed - _lastDatabase).Seconds >= 5)
                    {
                        _lastDatabase = timer.Elapsed;
                        if (_last != null &&
                            _isCorrectDatabase &&
                            Settings.OrionDatabase.Length != 0 &&
                            Settings.IsServiceRunning(Settings.OrionServerService))
                        {
                            CheckDatabase();
                        }
                    }

                    // check registry every 10 seconds
                    if ((timer.Elapsed - _lastRegistry).Seconds >= 10)
                    {
                        _lastRegistry = timer.Elapsed;
                        if (Settings.LoadRegistry())
                        {
                            _lastRequest = false;
                            _last = null;
                        }
                    }

                    await Task.Delay(1000);
                }
            });
        }

        public void SetLast(DateTime last)
        {
            _last = last;
            Log($"Last set to [ {_last} ]");
        }

        private void CheckDatabase()
        {
            // m_alarm
            // 37 - Пожар
            // 40 - Пожар 2
            // 44 - Внимание! Опасность пожара
            // 265 - Два пожара

            // pLogData
            // 130 - Включение насоса
            // 131 - Выключение насоса
            // 216 - Сработка датчика
            // 238 - Смена дежурства

            // m_alarm
            const string query1 = @"
                select
                m_alarm.GUID, m_alarm.Time0 Time, m_alarm.ZoneName Address,
                m_alarm.ObjectName AddressName, m_alarm.Object Section,
                pObjects.Name SectionName,
                Events.Contents Event
                from m_alarm
                join pObjects ON pObjects.GIndex = m_alarm.Object
                join Events ON Events.Event = m_alarm.Event
                where m_alarm.Time0 > @last and m_alarm.Event in (37, 40, 44, 265)
                order by m_alarm.Time0";

            // pLogData
            const string query2 = @"
                select
                pLogData.GUID, pLogData.TimeVal Time, 
                convert(varchar(11), pLogData.Par1) + '/' + convert(varchar(11), pLogData.Par2) + '/' + convert(varchar(11), pLogData.Par3) + '/' + convert(varchar(11), pLogData.Par4) Address,
                pLogData.Remark AddressName, pLogData.RazdIndex Section,
                pObjects.Name SectionName,
                Events.Contents Event
                from pLogData
                join pObjects ON pObjects.GIndex = pLogData.RazdIndex
                join Events ON Events.Event = pLogData.Event
                where pLogData.TimeVal > @last and pLogData.Event in (216)
                order by pLogData.TimeVal";

            var alarms = new List<Alarm>();

            try
            {
                using (var connection = new SqlConnection(Settings.ConnectionStringOrion))
                using (var command = new SqlCommand())
                {
                    connection.Open();

                    command.Connection = connection;
                    command.CommandText = query1;
                    command.Parameters.AddWithValue("@last", _last);
                    using (var reader = command.ExecuteReader())
                        if (reader.HasRows)
                            while (reader.Read())
                                alarms.Add(new Alarm(reader, Settings.OrionDatabase, Settings.ObjectName));

                    command.CommandText = query2;
                    using (var reader = command.ExecuteReader())
                        if (reader.HasRows)
                            while (reader.Read())
                                alarms.Add(new Alarm(reader, Settings.OrionDatabase, Settings.ObjectName));
                }
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case 208:
                        Except($"[ {Settings.OrionServer} \\ {Settings.OrionDatabase} ] not correct");
                        _isCorrectDatabase = false;
                        break;
                    case 4060:
                        Except($"[ {Settings.OrionServer} \\ {Settings.OrionDatabase} ] not found");
                        _isCorrectDatabase = false;
                        break;
                    default:
                        Except(ex);
                        break;
                }
            }
            catch (Exception ex)
            {
                Except(ex);
            }

            if (alarms.Count > 0)
            {
                // sort alarms by time
                alarms.Sort((a, b) => a.Time.CompareTo(b.Time));
                Log($"Found [ {alarms.Count} ] new alarms between [ {alarms[0].Time} ] and [ {alarms[alarms.Count - 1].Time} ]");
                SetLast(alarms[alarms.Count - 1].Time);
                AlarmsEvent?.Invoke(alarms);
            }
        }
    }
}

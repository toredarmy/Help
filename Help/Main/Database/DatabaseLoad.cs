using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using Help.Main.Extension;

namespace Help.Main.Database
{
    internal sealed partial class Database
    {
        public void GetLast()
        {
#if DEBUG
            var last = DateTime.ParseExact("01-01-1900", "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
#else
            var last = DateTime.Now;
#endif
            try
            {
                const string query = @"
                    select top 1 Time
                    from Alarms
                    where Src=@Src
                    order by Time desc";
                using (var connection = new SqlConnection(Settings.ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Src", Settings.OrionDatabase);
                    using (var reader = command.ExecuteReader())
                        if (reader.HasRows)
                            while (reader.Read())
                                last = reader.GetValue<DateTime>("Time");
                }
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            LastEvent?.Invoke(last);
        }

        public void GetNotSend()
        {
            try
            {
                const string query = "select * from Alarms where Send=0";
                using (var connection = new SqlConnection(Settings.ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                        if (reader.HasRows)
                        {
                            var alarms = new List<Alarm>(0);
                            while (reader.Read())
                            {
                                alarms.Add(new Alarm(reader));
                            }
                            if (alarms.Count > 0)
                            {
                                Log($"Found [ {alarms.Count} ] not send alarms");
                                AlarmsEvent?.Invoke(alarms);
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                Except(ex);
            }
        }
    }
}

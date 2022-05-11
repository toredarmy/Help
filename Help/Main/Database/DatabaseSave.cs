using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Help.Main.Database
{
    internal partial class Database
    {
        public void SaveAlarms(List<Alarm> alarms)
        {
            try
            {
                const string query = @"
                    insert into Alarms (
                    GUID, Time, Src,
                    Obj, Address, AddressName,
                    Section, SectionName, Event) 
                    output inserted.id
                    values (
                    @GUID, @Time, @Src, @Obj,
                    @Address, @AddressName, @Section,
                    @SectionName, @Event)";
                using (var connection = new SqlConnection(Settings.ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    var saved = 0;
                    var exists = 0;
                    var savedAlarms = new List<Alarm>(0);

                    connection.Open();

                    foreach (var alarm in alarms)
                    {
                        var result = SaveAlarm(command, alarm);
                        switch (result)
                        {
                            case 1:
                                saved++;
                                savedAlarms.Add(alarm);
                                break;
                            case 2:
                                exists++;
                                break;
                        }
                    }

                    if (exists > 0)
                    {
                        Log($"Saved [ {saved} ], exists [ {exists} ] alarms");
                    }
                    else if (saved > 0)
                    {
                        Log($"Saved [ {saved} ] alarms");
                        AlarmsEvent?.Invoke(savedAlarms);
                    }
                }
            }
            catch (Exception ex)
            {
                Except(ex);
            }
        }

        private int SaveAlarm(SqlCommand command, Alarm alarm)
        {
            try
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@GUID", alarm.Guid);
                command.Parameters.AddWithValue("@Time", alarm.Time);
                command.Parameters.AddWithValue("@Src", alarm.Database);
                command.Parameters.AddWithValue("@Obj", alarm.Object);
                command.Parameters.AddWithValue("@Address", alarm.Address);
                command.Parameters.AddWithValue("@AddressName", alarm.AddressName);
                command.Parameters.AddWithValue("@Section", alarm.Section);
                command.Parameters.AddWithValue("@SectionName", alarm.SectionName);
                command.Parameters.AddWithValue("@Event", alarm.Event);
                alarm.Id = (int)command.ExecuteScalar();
                return 1;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    return 2;
                }
                Except(ex);
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            return 0;
        }

        public void UpdateAlarms(List<Alarm> alarms)
        {
            try
            {
                var ids = alarms.ConvertAll(alarm => alarm.Id).ToArray();
                var query = $"update Alarms set Send=1 where Id in ({string.Join(",", ids)})";
                using (var connection = new SqlConnection(Settings.ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Except(ex);
            }
        }


        public void UpdateAlarm(Alarm alarm)
        {
            try
            {
                var query = $"update Alarms set Send=1 where Id={alarm.Id}";
                using (var connection = new SqlConnection(Settings.ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Except(ex);
            }
        }
    }
}

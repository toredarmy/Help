using System;
using System.Data.SqlClient;

namespace Help.Main.Database
{
    internal sealed partial class Database
    {
        private bool CreateDatabase()
        {
            try
            {
                var query = $"create database {Settings.Database}";
                using (var connection = new SqlConnection(Settings.ConnectionStringMaster))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                Log($"[ {Settings.Server} \\ {Settings.Database} ] create");
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 1801)
                {
                    Log($"[ {Settings.Server} \\ {Settings.Database} ] exist");
                    return true;
                }
                Except(ex);
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            return false;
        }

        private bool CreateTable()
        {
            try
            {
                const string query = @"
                    create table Alarms (
                    Id int identity(1, 1) primary key,
                    GUID uniqueidentifier,
                    Time datetime,
                    Src varchar(60),
                    Obj varchar(60),
                    Address varchar(60),
                    AddressName varchar(60),
                    Section int,
                    SectionName varchar(60),
                    Event varchar(60),
                    Send int default 0,
                    constraint AK_GUID unique(GUID))";
                using (var connection = new SqlConnection(Settings.ConnectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                Log($"[ {Settings.Server} \\ {Settings.Database}\\Alarms ] create");
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2714)
                {
                    Log($"[ {Settings.Server} \\ {Settings.Database}\\Alarms ] exist");
                    return true;
                }
                Except(ex);
            }
            catch (Exception ex)
            {
                Except(ex);
            }
            return false;
        }

        private void Delete()
        {
            try
            {
                var query = $@"
                    USE tempdb;
                    DECLARE @SQL nvarchar(1000);
                    IF EXISTS (SELECT 1 FROM sys.databases WHERE [name] = N'{Settings.Database}')
                    BEGIN
                        SET @SQL = N'USE [{Settings.Database}];
                        ALTER DATABASE {Settings.Database} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        USE [tempdb];
                        DROP DATABASE {Settings.Database};';
                        EXEC (@SQL);
                    END;";
                using (var connection = new SqlConnection(Settings.ConnectionStringMaster))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                Log($"[ {Settings.Server} \\ {Settings.Database} ] delete");
            }
            catch (Exception ex)
            {
                Except(ex);
            }
        }
    }
}

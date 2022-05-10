using System;
using System.Data.SqlClient;

namespace Help.Main.Extension
{
    internal static class SqlDataReaderExtension
    {
        public static T GetValue<T>(this SqlDataReader sqlDataReader, string columnName) =>
            sqlDataReader[columnName] == DBNull.Value ? default : (T)sqlDataReader[columnName];
    }
}

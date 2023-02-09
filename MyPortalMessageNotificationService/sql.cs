using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SqlCommand = System.Data.SqlClient.SqlCommand;
using SqlConnection = System.Data.SqlClient.SqlConnection;
using SqlParameter = System.Data.SqlClient.SqlParameter;

namespace MyPortalMessageNotificationService
{
    public class Sql
    {
        public static int ExecuteNonQuery(string connectionString, string sql, int commandTimeout = 30)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return new SqlCommand(sql, connection) { CommandTimeout = commandTimeout }.ExecuteNonQuery();
            }
        }

        public static int ExecuteNonQuery(string connectionString, string sql, IEnumerable<SqlParameter> parameters, int commandTimeout = 30)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(sql, connection) { CommandTimeout = commandTimeout };

                command.Parameters.AddRange(parameters.ToArray());
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static IEnumerable<DataRow> ExecuteQuery(string connectionString, string sql, int commandTimeout = 30)
        {
            return ExecuteQuery(connectionString, sql, new List<SqlParameter>(), commandTimeout);
        }

        public static IEnumerable<DataRow> ExecuteQuery(string connectionString, string sql, IEnumerable<SqlParameter> parameters, int commandTimeout = 30)
        {
            var dataSet = new DataSet();

            using (var connection = new SqlConnection(connectionString))
            {
                var adapter = new SqlDataAdapter(sql, connection);

                connection.Open();
                adapter.SelectCommand.CommandTimeout = commandTimeout;
                adapter.SelectCommand.Parameters.AddRange(parameters.ToArray());
                adapter.Fill(dataSet);
                return dataSet.Tables[0].Rows.Cast<DataRow>();
            }
        }

        public static long ExecuteScalar(string connectionString, string sql, int commandTimeout = 30)
        {
            return ExecuteScalar<long>(connectionString, sql, commandTimeout);
        }

        public static T ExecuteScalar<T>(string connectionString, string sql, int commandTimeout = 30)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var value = new SqlCommand(sql, connection) { CommandTimeout = commandTimeout }.ExecuteScalar();
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }
    }
}
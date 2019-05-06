using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dapper;

namespace ConsoleTestProgram
{
    static class Program
    {
        static void Main(string[] args)
        {
            var connector = new DbConnector("EMP-CMSDB02");

            const string query = "SELECT AktivId AS Id, Aktiv AS Active FROM conf_Aktiv;"; // Query waits 10 seconds...

            Console.WriteLine("Data reading with build in methods");
            connector.ExecuteQuery(query, ReadData);

            Console.WriteLine("Date reading with dapper");
            connector.ExecuteQueryDapper(query, ReadDataDapper);

            Console.WriteLine("Done...");
            Console.ReadLine();
        }

        /// <summary>
        /// Reads the data with the build in methods
        /// </summary>
        /// <param name="reader">The sql data reader</param>
        private static void ReadData(SqlDataReader reader)
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Console.WriteLine($"Id: {reader["Id"]} - Value: {reader["Active"]}");
                }
            }
        }

        /// <summary>
        /// Reads the data with dapper
        /// </summary>
        /// <param name="connection">The sql connection</param>
        /// <param name="query">The query</param>
        private static void ReadDataDapper(SqlConnection connection, string query)
        {
            var result = connection.Query<ActiveType>(query).ToList();

            foreach (var entry in result)
            {
                Console.WriteLine($"Id: {entry.Id} - Value: {entry.Active}");
            }
        }
    }

    // Class for the dapper result
    internal class ActiveType
    {
        public int Id { get; set; }
        public string Active { get; set; }
    }

    // Class to create a connection to the given sql data base
    public class DbConnector
    {
        /// <summary>
        /// Contains the name of the server
        /// </summary>
        private readonly string _server;

        /// <summary>
        /// Creates a new instance of the server
        /// </summary>
        /// <param name="server"></param>
        public DbConnector(string server)
        {
            _server = server;
        }

        /// <summary>
        /// Creates a new connection
        /// </summary>
        private SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(new SqlConnectionStringBuilder
            {
                DataSource = _server,
                InitialCatalog = "EMP_CMS",
                IntegratedSecurity = true
            }.ConnectionString);

            return connection;
        }

        /// <summary>
        /// Executes the given query
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="action">The action which should be executed</param>
        /// <returns>The sql data reader</returns>
        public void ExecuteQuery(string query, Action<SqlDataReader> action)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var cmd = new SqlCommand(query, connection))
                {
                    action(cmd.ExecuteReader());
                }
            }
        }

        /// <summary>
        /// Creates a new connection and executes the action
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="action">The action</param>
        public void ExecuteQueryDapper(string query, Action<SqlConnection, string> action)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                action(connection, query);
            }
        }
    }
}

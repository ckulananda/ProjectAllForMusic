using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProjectAllForMusic.Model
{
    public class DBConnection
    {
        private SqlConnection connection;

        public DBConnection()
        {
            var connectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build()
                .GetSection("ConnectionStrings")["scon"];

            connection = new SqlConnection(connectionString);
        }

        public SqlConnection GetConn()
        {
            return connection;
        }

        public void ConOpen()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }
        }

        public void ConClose()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }
}

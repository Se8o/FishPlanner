using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishPlanner.Database
{
    public static class DbManager
    {
        private static readonly string connectionString = @"Server=193.85.203.188;Database=lodin;User Id=lodin;Password=Sebast8008;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}

using System;
using System.Data.SqlClient;

namespace QuizServer
{
    public static class DBConnect
    {
        private static readonly string connectionString =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=DB_AILATRIEUPHU;Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            Console.WriteLine("Dang tao ket noi DB: " + connectionString);
            return new SqlConnection(connectionString);
        }
    }
}
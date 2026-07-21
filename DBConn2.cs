using DocumentFormat.OpenXml.Office.Word;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;

namespace Excel
{
    public class SqlConn2
    {
        private readonly string _connectionString;

        public SqlConn2(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EmployeeDb")
                ?? throw new ArgumentException("Connection string is required.");
        }

        public void SaveAnalysis(string referenceNumber, double mean)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Analysis (ReferenceNumber, Mean) VALUES (@ReferenceNumber, @Mean)";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ReferenceNumber", referenceNumber);
                    cmd.Parameters.AddWithValue("@Mean", mean.ToString());
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
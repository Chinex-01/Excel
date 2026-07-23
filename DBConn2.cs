using DocumentFormat.OpenXml.Office.Word;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;


namespace Excel
{
    public class SqlConn2
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlConn2> _logger;

        public SqlConn2(IConfiguration configuration, ILogger<SqlConn2> logger)
        {
            const string method = nameof(SqlConn2);
            _logger = logger;
            try
            {
                _connectionString = configuration.GetConnectionString("EmployeeDb")
                    ?? throw new ArgumentException("Connection string is required.");
                _logger.LogInformation("[SqlConn2.{Method}] Connection string loaded.", method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SqlConn2.{Method}] Failed loading connection string.", method);
                throw;
            }
        }

        public void SaveAnalysis(string referenceNumber, double mean)
        {
            const string method = nameof(SaveAnalysis);
            try
            {
                _logger.LogInformation("[SqlConn2.{Method}] Saving analysis. ReferenceNumber={ReferenceNumber}, Mean={Mean}",
                    method, referenceNumber, mean);

                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                _logger.LogInformation("[SqlConn2.{Method}] DB connection opened.", method);

                string query = @"INSERT INTO Analysis (ReferenceNumber, Mean) VALUES (@ReferenceNumber, @Mean)";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ReferenceNumber", referenceNumber);
                cmd.Parameters.AddWithValue("@Mean", mean.ToString());

                _logger.LogInformation("[SqlConn2.{Method}] Sending INSERT to DB.", method);
                int rowsAffected = cmd.ExecuteNonQuery();
                _logger.LogInformation("[SqlConn2.{Method}] DB write done. RowsAffected={RowsAffected}", method, rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SqlConn2.{Method}] Failed saving analysis. ReferenceNumber={ReferenceNumber}", method, referenceNumber);
                throw;
            }
        }
    }
}
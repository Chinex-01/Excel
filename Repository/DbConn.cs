using Microsoft.Data.SqlClient;
using System.Data;

namespace Excel
{
    public class SqlConn
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlConn> _logger;

        public SqlConn(IConfiguration configuration, ILogger<SqlConn> logger)
        {
            const string method = nameof(SqlConn);
            _logger = logger;
            try
            {
                _connectionString = configuration.GetConnectionString("EmployeeDb")
                    ?? throw new ArgumentException("Connection string is required.");
                _logger.LogInformation("[SqlConn.{Method}] Connection string loaded.", method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SqlConn.{Method}] Failed loading connection string.", method);
                throw;
            }
        }

        public string DbConn(List<Employee> employees, string referenceNumber, double mean)
        {
            const string method = nameof(DbConn);
            try
            {
                _logger.LogInformation(
                    "[SqlConn.{Method}] Starting save. EmployeeCount={Count}, ReferenceNumber={ReferenceNumber}, Mean={Mean}",
                    method, employees?.Count ?? 0, referenceNumber, mean);

                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                _logger.LogInformation("[SqlConn.{Method}] DB connection opened.", method);

                using var transaction = connection.BeginTransaction();
                try
                {
                    // "DbConn" logic — bulk employee insert
                    string query1 = @"INSERT INTO Excel_sheet (Employ_id, Username, Age, Grade, Department)
                                      VALUES (@Employ_id, @Username, @Age, @Grade, @Department)";

                    using var cmd1 = new SqlCommand(query1, connection, transaction);
                    cmd1.Parameters.Add("@Employ_id", SqlDbType.VarChar, 5);
                    cmd1.Parameters.Add("@Username", SqlDbType.NVarChar);
                    cmd1.Parameters.Add("@Age", SqlDbType.Int);
                    cmd1.Parameters.Add("@Grade", SqlDbType.NVarChar);
                    cmd1.Parameters.Add("@Department", SqlDbType.NVarChar);

                    foreach (var emp in employees)
                    {
                        cmd1.Parameters["@Employ_id"].Value = emp.Employ_id;
                        cmd1.Parameters["@Username"].Value = emp.Username;
                        cmd1.Parameters["@Age"].Value = emp.Age;
                        cmd1.Parameters["@Grade"].Value = emp.Grade;
                        cmd1.Parameters["@Department"].Value = emp.Department;

                        _logger.LogInformation("[SqlConn.{Method}] Sending INSERT for Employ_id={EmployId}", method, emp.Employ_id);
                        cmd1.ExecuteNonQuery();
                        _logger.LogInformation("[SqlConn.{Method}] Insert OK for Employ_id={EmployId}", method, emp.Employ_id);
                    }

                 
                    string query2 = @"INSERT INTO Analysis (ReferenceNumber, Mean) VALUES (@ReferenceNumber, @Mean)";

                    using var cmd2 = new SqlCommand(query2, connection, transaction);
                    cmd2.Parameters.AddWithValue("@ReferenceNumber", referenceNumber);
                    cmd2.Parameters.AddWithValue("@Mean", mean.ToString());

                    _logger.LogInformation("[SqlConn.{Method}] Sending INSERT for analysis. ReferenceNumber={ReferenceNumber}", method, referenceNumber);
                    int rowsAffected = cmd2.ExecuteNonQuery();
                    _logger.LogInformation("[SqlConn.{Method}] Analysis insert OK. RowsAffected={RowsAffected}", method, rowsAffected);

                    transaction.Commit();
                    _logger.LogInformation("[SqlConn.{Method}] Transaction committed.", method);

                    return "Excel uploaded and analysis saved to database";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "[SqlConn.{Method}] Transaction rolled back.", method);
                    throw;
                }
            }
            catch (SqlException sqlex)
            {
                _logger.LogError(sqlex, "[SqlConn.{Method}] Failed saving employees/analysis to DB.", method);
                throw;
            }
        }
    }
}
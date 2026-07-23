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

        public string DbConn(List<Employee> employees)
        {
            const string method = nameof(DbConn);
            try
            {
                _logger.LogInformation("[SqlConn.{Method}] Starting bulk insert. EmployeeCount={Count}", method, employees?.Count ?? 0);

                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                _logger.LogInformation("[SqlConn.{Method}] DB connection opened.", method);

                string query = @"INSERT INTO Excel_sheet (Employ_id, Username, Age, Grade, Department)
                                  VALUES (@Employ_id, @Username, @Age, @Grade, @Department)";

                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.Add("@Employ_id", SqlDbType.VarChar, 5);
                cmd.Parameters.Add("@Username", SqlDbType.NVarChar);
                cmd.Parameters.Add("@Age", SqlDbType.Int);
                cmd.Parameters.Add("@Grade", SqlDbType.NVarChar);
                cmd.Parameters.Add("@Department", SqlDbType.NVarChar);

                foreach (var emp in employees)
                {
                    try
                    {
                        cmd.Parameters["@Employ_id"].Value = emp.Employ_id;
                        cmd.Parameters["@Username"].Value = emp.Username;
                        cmd.Parameters["@Age"].Value = emp.Age;
                        cmd.Parameters["@Grade"].Value = emp.Grade;
                        cmd.Parameters["@Department"].Value = emp.Department;

                        _logger.LogInformation("[SqlConn.{Method}] Sending INSERT for Employ_id={EmployId}", method, emp.Employ_id);
                        cmd.ExecuteNonQuery();
                        _logger.LogInformation("[SqlConn.{Method}] Insert OK for Employ_id={EmployId}", method, emp.Employ_id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[SqlConn.{Method}] Insert failed for Employ_id={EmployId}", method, emp.Employ_id);
                        throw;
                    }
                }

                _logger.LogInformation("[SqlConn.{Method}] Bulk insert completed.", method);
                return "Excel uploaded and saved to database";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SqlConn.{Method}] Failed saving employees to DB.", method);
                throw;
            }
        }
    }
}
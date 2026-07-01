using System.Data;
using System.Data.Common;
using Excel;
using Microsoft.Data.SqlClient;
using List<Employee> employees = new List<Employee>();

namespace Excel
{
    public class Sqlconn
    {
        public static DbConn ()
        {
             string connectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=Employee;Trusted_Connection=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = @"INSERT INTO Excel_sheet (Employ_id, Username, Age, Grade, Department) VALUES (@Employ_id ,@Username, @Age, @Grade, @Department)";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                foreach (var emp in employees)
                {
                    cmd.Parameters.AddWithValue("@Employ_id ", emp.Employ_id);
                    cmd.Parameters.AddWithValue("@Username", emp.Username);
                    cmd.Parameters.AddWithValue("@Age", emp.Age);
                    cmd.Parameters.AddWithValue("@Grade", emp.Grade);
                    cmd.Parameters.AddWithValue("@Department", emp.Department);

                    cmd.ExecuteNonQuery();

                }
            }
        }
        }
    }
}
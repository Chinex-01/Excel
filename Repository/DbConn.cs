using ClosedXML.Excel;
using Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.ComponentModel;
using System.Data;

namespace Excel
{
    public class Sqlconn
    {
        public static string DbConn (List<Employee> employees)
        {
             string connectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=Employee;Trusted_Connection=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = @"INSERT INTO Excel_sheet (Employ_id, Username, Age, Grade, Department) VALUES (@Employ_id ,@Username, @Age, @Grade, @Department)";

            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                    cmd.Parameters.Add("@Employ_id", SqlDbType.VarChar, 5);
                    cmd.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar);
                    cmd.Parameters.Add("@Age", System.Data.SqlDbType.Int);
                    cmd.Parameters.Add("@Grade", System.Data.SqlDbType.NVarChar);
                    cmd.Parameters.Add("@Department", System.Data.SqlDbType.NVarChar);

                    foreach (var emp in employees)
                    {
                        cmd.Parameters["@Employ_id"].Value = emp.Employ_id;
                        cmd.Parameters["@Username"].Value = emp.Username;
                        cmd.Parameters["@Age"].Value = emp.Age;
                        cmd.Parameters["@Grade"].Value = emp.Grade;
                        cmd.Parameters["@Department"].Value = emp.Department;

                        cmd.ExecuteNonQuery();
                    }
            }
        }
             return ("Excel uploaded and saved to database");
        }
    }
}
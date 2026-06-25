using Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Core.ExcelPackage;
using System.ComponentModel;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;


[ApiController]
[Route("api/report")]
public class UploadExcelController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload_Excel(IFormFile file)
    {
        List<Employee> employees = new List<Employee>();

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Folder location
        var folderPath = @"C:\Users\onyeo\Desktop\Excel\wwwroot\upload";

        // Create folder if missing
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Full file path
        var filePath = Path.Combine(folderPath, file.FileName);

        await using var stream = new FileStream(filePath, FileMode.Create);

        await file.CopyToAsync(stream);

        return Ok("Upload successful");

        string connectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=Employee;Trusted_Connection=True;";


        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = @"INSERT INTO Excel_sheet (Username, Age, Grade, Department) VALUES (@Username, @Age, @Grade, @Department)";


            using (SqlCommand cmd = new SqlCommand(query, connection))
            {


                foreach (var emp in employees)
                {
                    cmd.Parameters.AddWithValue("@Username", emp.Username);
                    cmd.Parameters.AddWithValue("@Age", emp.Age);
                    cmd.Parameters.AddWithValue("@Grade", emp.Grade);
                    cmd.Parameters.AddWithValue("@Department", emp.Department);

                    cmd.ExecuteNonQuery();
                }

            }

            return Ok("Excel uploaded and saved to database");
        }
    }
}

    



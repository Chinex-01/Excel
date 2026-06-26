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

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension.Rows;

            // Start at row 2 assuming row 1 = headers
            for (int row = 2; row <= rowCount; row++)
            {
                employees.Add(new Employee
                {
                    Username = worksheet.Cells[row, 1].Text,
                    Age = int.Parse(worksheet.Cells[row, 2].Text),
                    Grade = worksheet.Cells[row, 3].Text,
                    Department = worksheet.Cells[row, 4].Text
                });
            }
        }
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







using Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.ComponentModel;
using ClosedXML.Excel;

[ApiController]
[Route("api/report")]
public class UploadExcelController : ControllerBase
{
    List<Employee> employees = new List<Employee>();
    [HttpPost("upload")]
    public async Task<IActionResult> Upload_Excel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
              return BadRequest("No file uploaded");
        }
        var allowedExtensions = new[] { ".xlsx", ".xls" };

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Only Excel files (.xlsx, .xls) are allowed.");
        }

        // Validate Excel columns
        var requiredColumns = new List<string>
        {
            "employ id",
            "username",
            "age",
            "grade",
            "department"
        };

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

        Read.Reader(employees, filePath);
        Sqlconn.DbConn(employees);

        return Ok("Excel uploaded and saved to database");
    }
}


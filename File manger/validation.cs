using Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.ComponentModel;
using ClosedXML.Excel;

namespace Excel
{
    public class validation
    {
        public static async Task  Rain(IFormFile file )
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception( "No file uploaded");
            }
            var allowedExtensions = new[] { ".xlsx", ".xls" };

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Only Excel files (.xlsx, .xls) are allowed.");
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
        }
    }
}
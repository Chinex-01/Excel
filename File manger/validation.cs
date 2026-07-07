using ClosedXML.Excel;
using System.Data;

namespace Excel
{
    public class Validation
    {
        public static async Task<string> Rain(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("No file uploaded.");
            }

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Only Excel files (.xlsx, .xls) are allowed.");
            }

            var filePath = Path.Combine("upload", file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }
        public static bool ValidateExcelHeader(IXLWorksheet worksheet)
        {
            var requiredColumns = new List<string>
            {
                "Employ_id",
                "Username",
                "Age",
                "Grade",
                "Department"
            };

            if (worksheet.Row(1).CellsUsed().Count() != requiredColumns.Count)
            {
                return false;
            }
            for (int i = 0; i < requiredColumns.Count; i++)
            {
                string excelHeader = worksheet.Cell(1, i + 1).GetString().Trim().ToLower();
                string expectedHeader = requiredColumns[i].Trim().ToLower();

                if (excelHeader != expectedHeader)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
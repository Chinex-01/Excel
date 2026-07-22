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

                if (!requiredColumns.Any(c => c.Trim().ToLower().ToString() == excelHeader))
                {
                    return false;
                }
            }
            return true;
        }
           public static double CalculateMeanAge(IXLWorksheet worksheet)
          {
            // Find which column index "Age" is in (in case column order isn't fixed)
            int ageColumnIndex = -1;
            var headerRow = worksheet.Row(1);

            foreach (var cell in headerRow.CellsUsed())
            {
                if (cell.GetString().Trim().Equals("Age", StringComparison.OrdinalIgnoreCase))
                {
                    ageColumnIndex = cell.Address.ColumnNumber;
                    break;
                }
            }
            if (ageColumnIndex == -1)
            {
                throw new Exception("Age column not found in worksheet.");
            }

              var ages = new List<double>();
              int lastRow = worksheet.LastRowUsed().RowNumber();

            for (int row = 2; row <= lastRow; row++)
            {
                var cell = worksheet.Cell(row, ageColumnIndex);

                if (!cell.IsEmpty() && cell.TryGetValue(out double ageValue))
                {
                    ages.Add(ageValue);
                }
            }

            if (ages.Count == 0)
            {
                throw new Exception("No valid Age values found to calculate mean.");
            }

            return ages.Average();
        }

    }
}

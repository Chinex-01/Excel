using ClosedXML.Excel;
using System.Data;

namespace Excel
{
    public class Validation
    {
        private readonly ILogger<Validation> _logger;

        public Validation(ILogger<Validation> logger)
        {
            _logger = logger;
        }

        public async Task<string> Rain(IFormFile file)
        {
            const string method = nameof(Rain);
            try
            {
                _logger.LogInformation("[Validation.{Method}] Started. FileName={FileName}", method, file?.FileName);

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("[Validation.{Method}] No file uploaded.", method);
                    throw new Exception("No file uploaded.");
                }

                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("[Validation.{Method}] Invalid extension: {Extension}", method, extension);
                    throw new Exception("Only Excel files (.xlsx, .xls) are allowed.");
                }

                var filePath = Path.Combine("upload", file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("[Validation.{Method}] File saved at {FilePath}", method, filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Validation.{Method}] Failed processing upload.", method);
                throw;
            }
        }

        public bool ValidateExcelHeader(IXLWorksheet worksheet)
        {
            const string method = nameof(ValidateExcelHeader);
            try
            {
                _logger.LogInformation("[Validation.{Method}] Validating header.", method);

                var requiredColumns = new List<string> { "Employ_id", "Username", "Age", "Grade", "Department" };

                if (worksheet.Row(1).CellsUsed().Count() != requiredColumns.Count)
                {
                    _logger.LogWarning("[Validation.{Method}] Column count mismatch. Expected={Expected}, Actual={Actual}",
                        method, requiredColumns.Count, worksheet.Row(1).CellsUsed().Count());
                    return false;
                }

                for (int i = 0; i < requiredColumns.Count; i++)
                {
                    string excelHeader = worksheet.Cell(1, i + 1).GetString().Trim().ToLower();
                    if (!requiredColumns.Any(c => c.Trim().ToLower() == excelHeader))
                    {
                        _logger.LogWarning("[Validation.{Method}] Unexpected header: {Header}", method, excelHeader);
                        return false;
                    }
                }

                _logger.LogInformation("[Validation.{Method}] Header validation passed.", method);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Validation.{Method}] Failed validating header.", method);
                throw;
            }
        }

        public double CalculateMeanAge(IXLWorksheet worksheet)
        {
            const string method = nameof(CalculateMeanAge);
            try
            {
                _logger.LogInformation("[Validation.{Method}] Calculating mean age.", method);

                int ageColumnIndex = -1;
                foreach (var cell in worksheet.Row(1).CellsUsed())
                {
                    if (cell.GetString().Trim().Equals("Age", StringComparison.OrdinalIgnoreCase))
                    {
                        ageColumnIndex = cell.Address.ColumnNumber;
                        break;
                    }
                }

                if (ageColumnIndex == -1)
                {
                    _logger.LogWarning("[Validation.{Method}] Age column not found.", method);
                    throw new Exception("Age column not found in worksheet.");
                }

                var ages = new List<double>();
                int lastRow = worksheet.LastRowUsed().RowNumber();

                for (int row = 2; row <= lastRow; row++)
                {
                    var cell = worksheet.Cell(row, ageColumnIndex);
                    if (!cell.IsEmpty() && cell.TryGetValue(out double ageValue))
                        ages.Add(ageValue);
                }

                if (ages.Count == 0)
                {
                    _logger.LogWarning("[Validation.{Method}] No valid Age values found.", method);
                    throw new Exception("No valid Age values found to calculate mean.");
                }

                var mean = ages.Average();
                _logger.LogInformation("[Validation.{Method}] Mean age = {Mean}", method, mean);
                return mean;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Validation.{Method}] Failed calculating mean age.", method);
                throw;
            }
        }
    }
}
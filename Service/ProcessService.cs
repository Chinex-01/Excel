using ClosedXML.Excel;
using Microsoft.Data.SqlClient;

namespace Excel.Service
{
    public class ProcessService
    {
        private readonly SqlConn _sqlConn;
        private readonly SqlConn2 _sqlConn2;
        private readonly Validation _validation;
        private readonly ReferenceNumberGenerate _refGen;
        private readonly ILogger<ProcessService> _logger;

        public ProcessService(
            SqlConn sqlConn,
            SqlConn2 sqlConn2,
            Validation validation,
            ReferenceNumberGenerate refGen,
            ILogger<ProcessService> logger)
        {
            _sqlConn = sqlConn;
            _sqlConn2 = sqlConn2;
            _validation = validation;
            _refGen = refGen;
            _logger = logger;
        }

        public async Task<string> ProcessExcelUpload(IFormFile file)
        {
            const string method = nameof(ProcessExcelUpload);
            string referenceNumber = null;

            try
            {
                _logger.LogInformation("[ProcessService.{Method}] Upload started. FileName={FileName}", method, file?.FileName);

                // Save uploaded file
                var filePath = await _validation.Rain(file);

                // Open the Excel file
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);

                // Validate header
                bool isValid = _validation.ValidateExcelHeader(worksheet);

                if (!isValid)
                {
                    _logger.LogWarning("[ProcessService.{Method}] Invalid Excel header. FileName={FileName}", method, file.FileName);
                    throw new InvalidHeaderException("Check your file.");
                }

                referenceNumber = _refGen.Generate();
                double mean = _validation.CalculateMeanAge(worksheet);

                _sqlConn2.SaveAnalysis(referenceNumber, mean);

                List<Employee> employees = new List<Employee>();
                var result = Read.Reader(employees, filePath);

                _sqlConn.DbConn(result);

                _logger.LogInformation(
                    "[ProcessService.{Method}] Excel upload succeeded. FileName={FileName}, ReferenceNumber={ReferenceNumber}",
                    method, file.FileName, referenceNumber);

                return "Excel uploaded and saved to database.";
            }
            catch (InvalidHeaderException)
            {
                throw;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx,
                    "[ProcessService.{Method}] Database error while processing uploaded Excel file. FileName={FileName}, ReferenceNumber={ReferenceNumber}",
                    method, file?.FileName, referenceNumber);
                throw new ProcessException("A database error occurred while saving the data.", sqlEx);
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx,
                    "[ProcessService.{Method}] File I/O error while processing uploaded Excel file. FileName={FileName}",
                    method, file?.FileName);
                throw new ProcessException("An error occurred while reading the file.", ioEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[ProcessService.{Method}] Unexpected error while processing uploaded Excel file. FileName={FileName}",
                    method, file?.FileName);
                throw new ProcessException("An unexpected error occurred while processing the file.", ex);
            }
        }
    }

    public class InvalidHeaderException : Exception
    {
        public InvalidHeaderException(string message) : base(message) { }
    }

    public class ProcessException : Exception
    {
        public ProcessException(string message, Exception inner) : base(message, inner) { }
    }
}
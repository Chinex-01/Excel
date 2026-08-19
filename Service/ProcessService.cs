using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Excel.Service
{
    public class ProcessService
    {
        private readonly SqlConn _sqlConn;
        private readonly Validation _validation;
        private readonly ReferenceNumberGenerate _refGen;
        private readonly ILogger<ProcessService> _logger;

        public ProcessService(
            SqlConn sqlConn,
            Validation validation,
            ReferenceNumberGenerate refGen,
            ILogger<ProcessService> logger)
        {
            _sqlConn = sqlConn;
            _validation = validation;
            _refGen = refGen;
            _logger = logger;
        }

        public async Task<string> ProcessExcelUpload(IFormFile file, string username, string requestId)
        {
            const string method = nameof(ProcessExcelUpload);
            string referenceNumber = null;

            try
            {
                // A request id is required for every upload and must be two letters
                // followed by two numbers, e.g. "AB12".
                if (string.IsNullOrWhiteSpace(requestId) ||
                    !Regex.IsMatch(requestId, "^[A-Za-z]{2}[0-9]{2}$"))
                {
                    _logger.LogWarning("[ProcessService.{Method}] Invalid request id. RequestId={RequestId}", method, requestId);
                    throw new InvalidRequestIdException("Request id must be two letters followed by two numbers, e.g. AB12.");
                }

                // Save uploaded file (validates it is a non-empty .xlsx/.xls).
                var filePath = await _validation.Rain(file);

                _logger.LogInformation("[ProcessService.{Method}] Upload started. RequestId={RequestId}, FileName={FileName}", method, requestId, file?.FileName);
                if (_sqlConn.IsDuplicateRequestToday(requestId))
                {
                    _logger.LogWarning("[ProcessService.{Method}] Duplicate upload for today. RequestId={RequestId}", method, requestId);
                    throw new DuplicateRequestException("This request id has already been used today.");
                }

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

                List<Employee> employees = new List<Employee>();
                var result = Read.Reader(employees, filePath);

                _sqlConn.DbConn(employees, referenceNumber, mean );

                // Record the request only after a successful save, so a failed
                // upload does not block the user from legitimately retrying today.
                await _sqlConn.LogRequestAsync(requestId);

                _logger.LogInformation(
                    "[ProcessService.{Method}] Excel upload succeeded. RequestId={RequestId}, FileName={FileName}, ReferenceNumber={ReferenceNumber}",
                    method, requestId, file.FileName, referenceNumber);

                return "Excel uploaded and saved to database.";
            }
            catch (InvalidHeaderException)
            {
                throw;
            }
            catch (InvalidRequestIdException)
            {
                throw;
            }
            catch (DuplicateRequestException)
            {
                throw;
            }
            catch (SqlException sqlex) {
                _logger.LogError(sqlex,
                    "[ProcessService.{Method}] Database error while processing uploaded Excel file. FileName={FileName}, ReferenceNumber={ReferenceNumber}",
                    method, file?.FileName, referenceNumber);
                throw new ProcessException("A database error occurred while saving the data.", sqlex);
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
    public class DuplicateRequestException : Exception
    {
        public DuplicateRequestException(string message) : base(message) { }
    }
    public class InvalidRequestIdException : Exception
    {
        public InvalidRequestIdException(string message) : base(message) { }
    }
}
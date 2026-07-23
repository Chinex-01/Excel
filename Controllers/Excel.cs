using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Serilog;
using Serilog.Context;


[ApiController]
[Route("api/report")]
public class UploadExcelController : ControllerBase
{
    private readonly SqlConn _sqlConn;
    private readonly SqlConn2 _sqlConn2;
    private readonly Validation _validation;
    private readonly ReferenceNumberGenerate _refGen;
    private readonly ILogger<UploadExcelController> _logger;

    public UploadExcelController(
        SqlConn sqlConn,
        SqlConn2 sqlConn2,
        Validation validation,
        ReferenceNumberGenerate refGen,
        ILogger<UploadExcelController> logger)
    {
        _sqlConn = sqlConn;
        _sqlConn2 = sqlConn2;
        _validation = validation;
        _refGen = refGen;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload_Excel(IFormFile file)
    {
        const string method = nameof(Upload_Excel);
        string referenceNumber = null;

        try
        {
            _logger.LogInformation("[UploadExcelController.{Method}] Upload started. FileName={FileName}", method, file?.FileName);

            // Save uploaded file
            var filePath = await _validation.Rain(file);

            // Open the Excel file
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);

                // Validate header
                bool isValid = _validation.ValidateExcelHeader(worksheet);

                if (isValid)
                {
                    referenceNumber = _refGen.Generate();
                    double mean = _validation.CalculateMeanAge(worksheet);

                    _sqlConn2.SaveAnalysis(referenceNumber, mean);

                    List<Employee> employees = new List<Employee>();
                    var result = Read.Reader(employees, filePath);

                    _sqlConn.DbConn(result);

                    _logger.LogInformation(
                        "[UploadExcelController.{Method}] Excel upload succeeded. FileName={FileName}, ReferenceNumber={ReferenceNumber}",
                        method, file.FileName, referenceNumber);

                    return Ok("Excel uploaded and saved to database.");
                }
                else
                {
                    _logger.LogWarning("[UploadExcelController.{Method}] Invalid Excel header. FileName={FileName}", method, file.FileName);
                    return BadRequest("Check your file.");
                }
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx,
                "[UploadExcelController.{Method}] Database error while processing uploaded Excel file. FileName={FileName}, ReferenceNumber={ReferenceNumber}",
                method, file?.FileName, referenceNumber);
            return StatusCode(500, "A database error occurred while saving the data.");
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx,
                "[UploadExcelController.{Method}] File I/O error while processing uploaded Excel file. FileName={FileName}",
                method, file?.FileName);
            return StatusCode(500, "An error occurred while reading the file.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[UploadExcelController.{Method}] Unexpected error while processing uploaded Excel file. FileName={FileName}",
                method, file?.FileName);
            return StatusCode(500, "An unexpected error occurred while processing the file.");
        }
    }
}
5

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
    private readonly ILogger<UploadExcelController> _logger;


    public UploadExcelController(SqlConn sqlConn, SqlConn2 sqlConn2, ILogger<UploadExcelController> logger)
    {
        _sqlConn = sqlConn;
        _sqlConn2 = sqlConn2;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload_Excel(IFormFile file)
    {
        string referenceNumber = null;
        try
        {
            // Save uploaded file
            var filePath = await Validation.Rain(file);

            // Open the Excel file
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);

                // Validate header
                bool isValid = Validation.ValidateExcelHeader(worksheet);

                if (isValid)
                {
                     referenceNumber = ReferenceNumberGenerate.Generate();
                    double mean = Validation.CalculateMeanAge(worksheet);

                    _sqlConn2.SaveAnalysis(referenceNumber, mean);

                    List<Employee> employees = new List<Employee>();

                    var result = Read.Reader(employees, filePath);

                    _sqlConn.DbConn(result);

                    _logger.LogInformation("Excel upload succeeded. FileName: {FileName}, ReferenceNumber: {ReferenceNumber}",file.FileName, referenceNumber);

                    return Ok("Excel uploaded and saved to database.");
                }
                else
                {
                    _logger.LogWarning("Invalid Excel header. FileName: {FileName}", file.FileName);
                    return BadRequest("Check your file.");
                }
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "Database error while processing uploaded Excel file.");
            return StatusCode(500, "A database error occurred while saving the data.");
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "File I/O error while processing uploaded Excel file.");
            return StatusCode(500, " error occurred while reading the file.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing uploaded Excel file.");
            return StatusCode(500, "An unexpected error occurred while processing the file.");
        }

    }
}

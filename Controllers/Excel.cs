using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/report")]
public class UploadExcelController : ControllerBase
{
    private readonly SqlConn _sqlConn;
    private readonly SqlConn2 _sqlConn2;
    public UploadExcelController(SqlConn sqlConn, SqlConn2 sqlConn2)
    {
        _sqlConn = sqlConn;
        _sqlConn2 = sqlConn2;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload_Excel(IFormFile file)
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
                string referenceNumber = ReferenceNumberGenerate.Generate();
                double mean = Validation.CalculateMeanAge(worksheet);

                _sqlConn2.SaveAnalysis(referenceNumber, mean);

                List<Employee> employees = new List<Employee>();

                var result = Read.Reader(employees, filePath);

                _sqlConn.DbConn(result);

                return Ok("Excel uploaded and saved to database.");
            } 
            else
            {
                return BadRequest("check your file .");
            }
        }
    }
   
    }
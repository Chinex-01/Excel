using ClosedXML.Excel;
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
                List<Employee> employees = new List<Employee>();

                var result = Read.Reader(employees, filePath);

                Sqlconn.DbConn(result);

                return Ok("Excel uploaded and saved to database.");
            } 
            else
            {
                return BadRequest("check your file .");
            }
        }
    }
}
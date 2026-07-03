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
        List<Employee> employees = new List<Employee>();

        string filePath = await validation.Rain(file);

        Read.Reader(employees, filePath);

        Sqlconn.DbConn(employees);

        return Ok("Excel uploaded and saved to database");
    }
}


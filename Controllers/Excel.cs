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
    List<Employee> employees = new List<Employee>();
    [HttpPost("upload")]
    public async Task<IActionResult> Upload_Excel(IFormFile file)
    {

        string filePath = await validation.Rain(file, @"C:\Users\onyeo\Desktop\Excel\wwwroot\upload");
        Read.Reader(employees, filePath);
        Sqlconn.DbConn(employees);

        return Ok("Excel uploaded and saved to database");
    }
}


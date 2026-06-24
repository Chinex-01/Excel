using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/report")]
public class UploadExcelController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var path = @"C:\Users\onyeo\Documents\Book1.xlsx";

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return Ok("Upload successful");
    }
}
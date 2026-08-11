using Excel.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/report")]
[Authorize(Roles = "Admin,User")]

public class UploadExcelController : ControllerBase
{
    private readonly ProcessService _processService;
    private readonly ILogger<UploadExcelController> _logger;

    public UploadExcelController(ProcessService processService, ILogger<UploadExcelController> logger)
    {
        _processService = processService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload_Excel(IFormFile file)
    {
        const string method = nameof(Upload_Excel);

        var username = User.Identity?.Name;

        _logger.LogInformation(
            "[UploadExcelController.{Method}] Received upload request. Username={Username}, FileName={FileName}",
            method, username, file?.FileName);
        try
        {
            var message = await _processService.ProcessExcelUpload(file, username);
            return Ok(message);
        }
        catch (DuplicateRequestException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ProcessException ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
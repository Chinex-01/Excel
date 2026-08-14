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
        // Read the request id from the token (set at login) instead of asking again.
        var requestId = User.FindFirst("RequestId")?.Value;

        _logger.LogInformation(
            "[UploadExcelController.{Method}] Received upload request. Username={Username}, RequestId={RequestId}, FileName={FileName}",
            method, username, requestId, file?.FileName);
        try
        {
            var message = await _processService.ProcessExcelUpload(file, username, requestId);
            return Ok(message);
        }
        catch (InvalidRequestIdException ex)
        {
            return BadRequest(ex.Message);
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
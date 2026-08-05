using Excel.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/report")]
[Authorize(Roles = "Admin, User")]

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
    public async Task<IActionResult> Upload_Excel([FromForm] UploadExcelRequest request)
    {
        const string method = nameof(Upload_Excel);

        _logger.LogInformation(
            "[UploadExcelController.{Method}] Received upload request. RequestId={RequestId}, FileName={FileName}",
            method, request?.RequestId, request?.File?.FileName);

        if (request is null || request.RequestId == Guid.Empty)
        {
            return BadRequest("A valid RequestId is required.");
        }

        try
        {
            var message = await _processService.ProcessExcelUpload(request.File, request.RequestId);
            return Ok(message);
        }
        catch (InvalidHeaderException ex)
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
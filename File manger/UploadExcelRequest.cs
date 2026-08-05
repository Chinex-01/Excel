public class UploadExcelRequest
{
    public Guid RequestId { get; set; }

    public IFormFile File { get; set; }
}
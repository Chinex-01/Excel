namespace Excel
{
    public class ReferenceNumberGenerate
    {
        private readonly ILogger<ReferenceNumberGenerate> _logger;

        public ReferenceNumberGenerate(ILogger<ReferenceNumberGenerate> logger)
        {
            _logger = logger;
        }

        public string Generate()
        {
            const string method = nameof(Generate);
            try
            {
                string guidPart = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                string reference = $"REF-{guidPart}";

                _logger.LogInformation("[ReferenceNumberGenerate.{Method}] Generated {Reference}", method, reference);
                return reference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReferenceNumberGenerate.{Method}] Failed generating reference number.", method);
                throw;
            }
        }
    }
}
namespace Excel
{
    public class ReferenceNumberGenerate
    {
        public static string Generate()
        {
            string guidPart = Guid.NewGuid().ToString("N") 
                                   .Substring(0, 8)
                                   .ToUpper();

            return $"REF-{guidPart}";
        }
    }
}

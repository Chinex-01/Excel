using System.Security.Cryptography;
using System.Text;

namespace Excel
{
    public class Person
    {
        public string RequestId { get; set; }


        public static Person ForUpload(string username, byte[] fileContent)
        {
            if (fileContent == null || fileContent.Length == 0)
                throw new ArgumentException("File content is required to build a request id.", nameof(fileContent));

            var normalisedUser = (username ?? string.Empty).Trim().ToLowerInvariant();

            // Hash username + file bytes into 16 bytes, then treat that as a GUID.
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
            hasher.AppendData(Encoding.UTF8.GetBytes(normalisedUser));
            hasher.AppendData(fileContent);
            byte[] hash = hasher.GetHashAndReset();

            return new Person { RequestId = new Guid(hash).ToString() };
        }

    }
}

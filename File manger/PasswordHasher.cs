using System.Security.Cryptography;
using System.Text;

public static class PasswordHasher
{
    public static string ComputeHash(string password)
    {
        using SHA256 sha256 = SHA256.Create();

        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

        StringBuilder builder = new();

        foreach (byte b in hash)
            builder.Append(b.ToString("x2"));

        return builder.ToString();
    }
}
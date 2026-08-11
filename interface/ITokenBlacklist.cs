namespace Excel.Service
{
    public interface ITokenBlacklist
    {
        void Revoke(string token, DateTime expiresUtc);

        bool IsRevoked(string token);
    }
}

namespace Excel.Service
{
    public interface IConfigService
    {
        Task<LoginResult> LoginAsync(string username, string password);
    }
}
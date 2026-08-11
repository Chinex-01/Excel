namespace Excel.Service
{
    public interface ILogoutService
    {
        Task<string?> LogoutAsync(HttpContext httpContext);
    }
}
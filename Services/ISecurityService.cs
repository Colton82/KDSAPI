namespace KDSAPI.Services
{
    public interface ISecurityService
    {
        string HashPassword(string password);
        bool ValidatePassword(string password, string hashedPassword);
    }
}

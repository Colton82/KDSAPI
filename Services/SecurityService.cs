using System.Text;

namespace KDSAPI.Services
{
    public class SecurityService
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool ValidatePassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}

using System.Text;

namespace KDSAPI.Services
{
    /// <summary>
    /// Security service for hashing and validating passwords.
    /// </summary>
    public class SecurityService : ISecurityService
    {
        /// <summary>
        /// Hashes a password using BCrypt.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Validates a password against a hashed password using BCrypt.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
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

using Microsoft.AspNetCore.Mvc;

namespace KDSAPI.Data
{
    /// <summary>
    /// Interface for the UsersDAO class.
    /// </summary>
    public interface IUsersDAO
    {
        UserModel GetByID(int id);
        UserModel GetByUsername(string username);
        IActionResult Create(string username, string hashedPassword);
    }
}

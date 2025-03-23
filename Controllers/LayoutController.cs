using KDSAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KDSAPI.Controllers
{
    /// <summary>
    /// Controller for handling layout data.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class LayoutController : Controller
    {
        LayoutDAO _layoutDAO = new LayoutDAO();
        UsersDAO _userDAO = new UsersDAO();

        /// <summary>
        /// Retrieves the layout data for the specified user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetLayoutByUsername(string username)
        {
            var id = _userDAO.GetIdByUseraname(username);
            if (id.GetType() != typeof(int))
            {
                return StatusCode(500, "Failed to retrieve user ID.");
            }
            else
            {
                string[] stations = _layoutDAO.GetLayoutByUserId((int)id);
                return Ok(stations);
            }
        }

        /// <summary>
        /// Saves the layout data for the specified user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="layout"></param>
        /// <returns></returns>
        [HttpPost("save/{username}")]
        public IActionResult SaveLayout(string username, [FromBody] List<string> layout)
        {
            if (layout == null || layout.Count == 0)
            {
                return BadRequest("Layout data is required.");
            }

            string layoutString = string.Join(",", layout);
            bool success = _layoutDAO.SaveLayout(username, layoutString);

            if (success)
            {
                return Ok("Layout saved successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to save layout.");
            }
        }
    }
}

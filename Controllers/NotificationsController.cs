using Microsoft.AspNetCore.Mvc;

namespace buildone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        /// <summary>
        /// Temporary endpoint returning empty notifications until full implementation
        /// </summary>
        [HttpGet]
        public IActionResult GetNotifications()
        {
            // Return empty notifications structure to prevent 404 errors
            var response = new
            {
                unreadCount = 0,
                notifications = new object[0]
            };
            
            return Ok(response);
        }
    }
}
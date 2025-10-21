using System.Diagnostics;
using buildone.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using buildone.Data;

namespace buildone.Controllers
{
    [Authorize] // Require authentication for the main website
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Debug()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var roles = currentUser != null ? await _userManager.GetRolesAsync(currentUser) : new List<string>();
            
            var debugInfo = $@"
                <h2>Debug Information</h2>
                <p><strong>Is Authenticated:</strong> {isAuthenticated}</p>
                <p><strong>User Name:</strong> {User.Identity?.Name ?? "None"}</p>
                <p><strong>User Email:</strong> {currentUser?.Email ?? "None"}</p>
                <p><strong>User Full Name:</strong> {currentUser?.FullName ?? "None"}</p>
                <p><strong>Roles:</strong> {string.Join(", ", roles)}</p>
                <p><strong>Is in Administrator Role:</strong> {User.IsInRole("Administrator")}</p>
                <hr>
                <a href='/UserManagement'>Test UserManagement Link</a><br>
                <a href='/Account/Login'>Login Page</a><br>
                <a href='/'>Home Page</a>
            ";
            
            return Content(debugInfo, "text/html");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.Admin;
using SV22T1020065.Models;
using System.Diagnostics;

namespace SV22T1020065.Admin.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userData = User.GetUserData();
            ViewBag.UserData = userData;
            return View();
        }
    }
}

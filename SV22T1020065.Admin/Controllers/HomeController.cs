using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.Models;
using System.Diagnostics;

namespace SV22T1020065.Admin.Controllers
{
    /// <summary>
    /// Trang ch? c?a ?ng d?ng, hi?n th? các thông tin t?ng quan v? ho?t ??ng kinh doanh (doanh thu, s? l??ng ??n hàng, s? l??ng khách hàng, v.v.)
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Trang ch? c?a ?ng d?ng, hi?n th? các thông tin t?ng quan v? ho?t ??ng kinh doanh (doanh thu, s? l??ng ??n hàng, s? l??ng khách hàng, v.v.)
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}

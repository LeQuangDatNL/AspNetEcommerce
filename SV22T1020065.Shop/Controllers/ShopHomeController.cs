using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;

namespace SV22T1020065.Shop.Controllers
{
    public class ShopHomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy() => View();
    }
}
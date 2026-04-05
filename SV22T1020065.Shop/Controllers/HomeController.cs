using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;

namespace SV22T1020065.Shop.Controllers
{
    public class HomeController : Controller
    {
        private const int PAGE_SIZE = 12;

        public async Task<IActionResult> Index(ProductSearchInput input)
        {
            // Thiết lập các giá trị mặc định nếu chưa có
            input.PageSize = PAGE_SIZE;
            if (input.Page < 1) input.Page = 1;

            // Gọi Business Layer để lấy dữ liệu (Dựa trên file CatalogDataService bạn gửi)
            var model = await CatalogDataService.ListProductsAsync(input);

            // Lưu lại giá trị tìm kiếm để hiển thị trên ô Search
            ViewBag.SearchValue = input.SearchValue;

            return View(model);
        }

        public IActionResult Privacy() => View();
    }
}
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private const int PAGESIZE = 10;
        private const string SESSION_KEY = "CategorySearchInput";

        /// <summary>
        /// Giao diện chính - Load lần đầu
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            // Lấy lại cấu hình tìm kiếm cũ từ Session (nếu có)
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SESSION_KEY) ?? new PaginationSearchInput()
            {
                Page = page,
                PageSize = PAGESIZE,
                SearchValue = searchValue
            };

            ViewBag.SearchValue = input.SearchValue;
            return await Search(input);
        }

        /// <summary>
        /// Hàm xử lý AJAX - Chỉ trả về cái bảng dữ liệu (PartialView)
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await CatalogDataService.ListCategoriesAsync(input);
            // Lưu lại trạng thái tìm kiếm vào Session
            ApplicationContext.SetSessionData(SESSION_KEY, input);

            // QUAN TRỌNG: Trả về PartialView "Search"
            return PartialView("Search", result);
        }

        // ... các hàm Create, Edit, Save, Delete giữ nguyên ...
    }
}
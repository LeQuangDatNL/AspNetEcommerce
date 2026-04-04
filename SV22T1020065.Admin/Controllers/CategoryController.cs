using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;
using System;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private int PAGESIZE = ApplicationContext.PAGE_SIZE;
        private static readonly string SESSION_KEY = "CategorySearchInput";

        /// <summary>
        /// Giao diện chính
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SESSION_KEY)
                        ?? new PaginationSearchInput()
                        {
                            Page = page,
                            PageSize = PAGESIZE,
                            SearchValue = searchValue
                        };

            ViewBag.SearchValue = input.SearchValue;
            var result = await CatalogDataService.ListCategoriesAsync(input);
            return View(result);
        }

        /// <summary>
        /// AJAX search + phân trang
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await CatalogDataService.ListCategoriesAsync(input);

            ApplicationContext.SetSessionData(SESSION_KEY, input);

            return PartialView("Search", result);
        }

        /// <summary>
        /// Thêm mới
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            var model = new Category()
            {
                CategoryID = 0
            };

            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật loại hàng";
            var model = await CatalogDataService.GetCategoryAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Category data)
        {
            try
            {
                ViewBag.Title = data.CategoryID == 0 ? "Bổ sung loại hàng" : "Cập nhật loại hàng";

                // Validate
                if (string.IsNullOrWhiteSpace(data.CategoryName))
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên loại hàng không được để trống");

                // Nếu lỗi validate
                if (!ModelState.IsValid)
                    return View("Edit", data);

                // Lưu DB
                if (data.CategoryID == 0)
                    await CatalogDataService.AddCategoryAsync(data);
                else
                    await CatalogDataService.UpdateCategoryAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xóa
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var model = await CatalogDataService.GetCategoryAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        // POST: Thực hiện xóa danh mục
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await CatalogDataService.DeleteCategoryAsync(id);
            return RedirectToAction("Index");
        }
    }
}
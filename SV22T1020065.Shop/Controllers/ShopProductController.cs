using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;
using SV22T1020065.Shop.AppCodes;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020065.Shop.Controllers
{
    public class ShopProductController : Controller
    {
        private int PAGESIZE = ApplicationContext.PAGE_SIZE;
        private static readonly string SESSION_KEY = "ProductSearchInput";

        /// <summary>
        /// Giao diện chính (AJAX)
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "", int categoryID = 0, int supplierID = 0,
            decimal minPrice = 0,
            decimal maxPrice = 0
        )
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(SESSION_KEY)
                        ?? new ProductSearchInput()
                        {
                            Page = page,
                            PageSize = PAGESIZE,
                            SearchValue = searchValue ?? "",
                            CategoryID = categoryID,
                            SupplierID = supplierID,
                            MinPrice = minPrice,
                            MaxPrice = maxPrice
                        };

            // giữ lại filter cho UI
            ViewBag.SearchValue = input.SearchValue;
            ViewBag.CategoryID = input.CategoryID;
            ViewBag.SupplierID = input.SupplierID;
            ViewBag.MinPrice = input.MinPrice;
            ViewBag.MaxPrice = input.MaxPrice;
            ViewBag.SearchValue = input.SearchValue;
            var result = await CatalogDataService.ListProductsAsync(input);
            return View(result);
        }

        /// <summary>
        /// AJAX search + filter + phân trang
        /// </summary>
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);

            ApplicationContext.SetSessionData(SESSION_KEY, input);

            return PartialView("Search", result);
        }

        /// <summary>
        /// Xem chi tiết sản phẩm (AJAX)
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            var photos = await CatalogDataService.ListPhotosAsync(id);
            var attributes = await CatalogDataService.ListAttributesAsync(id);

            var model = new ProductDetailViewModel
            {
                Product = product,
                Photos = photos.Where(p => !p.IsHidden).OrderBy(p => p.DisplayOrder).ToList(),
                Attributes = attributes.OrderBy(a => a.DisplayOrder).ToList()
            };

            return PartialView("Details", model);
        }
    }
}

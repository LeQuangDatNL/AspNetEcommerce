using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    [Authorize]
    public class ProductController : Controller
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

            return PartialView("Search", result); // ❗ quan trọng
        }

        // ===== Các hàm khác giữ nguyên =====
        /// <summary>
        /// Tạo mới sản phẩm và các dữ liệu liên quan (thuộc tính, hình ảnh)
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View();
        }
        /// <summary>
        /// Cập nhật thông tin sản phẩm và các dữ liệu liên quan (thuộc tính, hình ảnh)
        /// </summary>
        /// <param name="id">Mã sản phẩm</param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {
            ViewBag.SupplierId = id;
            return View();
        }
        /// <summary>
        /// Xóa mặt hàng (thay đổi trạng thái của sản phẩm thành không còn kinh doanh)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            ViewBag.SupplierId = id;
            return View();
        }
        /// <summary>
        /// Hiển thị danh sách thuộc tính của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm</param>
        /// <returns></returns>
        public IActionResult ListAttributes(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }
        /// <summary>
        /// bổ sung thuộc tính mới cho sản phẩm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult CreateAttributes(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }

        /// <summary>
        /// chỉnh sửa thông tin thuộc tính của sản phẩm
        /// </summary>
        /// <param name="id">Mã mặt hàng</param>
        /// <param name="attributeId">Mã thuộc tính</param>
        /// <returns></returns>
        public IActionResult EditAttributes(int id, int attributeId)
        {
            ViewBag.ProductId = id;
            ViewBag.AttributeId = attributeId;
            return View();
        }
        /// <summary>
        /// Xóa thuộc tính của sản phẩm
        /// </summary>
        /// <param name="id">Ma Mặt hàng</param>
        /// <param name="attributeId">Mã thuộc tính</param>
        /// <returns></returns>
        public IActionResult DeleteAttributes(int id, int attributeId)
        {
            return RedirectToAction("ListAttributes", new { id = id });
        }
        /// <summary>
        /// Hiển thị danh sách hình ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phản</param>
        /// <returns></returns>
        public IActionResult ListPhoto(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }
        /// <summary>
        /// Bô sung hình ảnh mới cho sản phẩm
        /// </summary>
        /// <param name="id">Mã mặt hàng</param>
        /// <returns></returns>
        public IActionResult CreatePhoto(int id)
        {
            ViewBag.ProductId = id;
            return View();
        }
        /// <summary>
        /// Chỉnh sửa thông tin hình ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm</param>
        /// <param name="uploadPhoto">Mã ảnh cần cập nhật</param>
        /// <returns></returns>
        public IActionResult CreatePhoto(int id, IFormFile uploadPhoto)
        {
            return RedirectToAction("ListPhoto", new { id = id });
        }
        /// <summary>
        /// Xóa hình ảnh của sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm</param>
        /// <param name="uploadPhoto">Mã ảnh cần cập nhật</param>
        /// <returns></returns>
        public IActionResult EditPhoto(int id, int photoId)
        {
            ViewBag.ProductId = id;
            ViewBag.PhotoId = photoId;
            return View();
        }

    }
}

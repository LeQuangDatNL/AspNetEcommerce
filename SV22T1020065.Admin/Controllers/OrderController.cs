using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.Admin;
using SV22T1020065.Admin.AppCodes;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Sales;
using System.Buffers;
using System.Reflection;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    /// <summary>
    /// Các chức năng quản lý nghiệp vụ liên quan đến đơn hàng
    /// </summary>
    [Authorize(Roles ="${WebUserRoles.Administrator},{ WebUserRoles.DataManager}")]
    public class OrderController : Controller
    {
        private const string ProductSearch = "OrderSearchProductInput";
        /// <summary>
        /// Hiển thị danh sách đơn hàng
        /// </summary>
        public async Task<IActionResult> Index(string customerName = "", OrderStatusEnum? status = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? orderID = null)
        {
            // Chuẩn bị input tìm kiếm
            var input = new OrderSearchInput
            {
                CustomerName = customerName,            
                Status = status ?? OrderStatusEnum.New, 
                DateFrom = dateFrom,
                DateTo = dateTo,
                Page = 1,
                PageSize = 20
            };

            // Lấy danh sách đơn hàng
            var result = await SalesDataService.ListOrdersAsync(input);

            return View(result);
        }

        /// <summary>
        /// Tìm kiếm đơn hàng
        /// </summary>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            if (input == null)
            {
                input = new OrderSearchInput
                {
                    Page = 1,
                    PageSize = 20
                };
            }

            var result = await SalesDataService.ListOrdersAsync(input);
            return View("Search", result);
        }

        /// <summary>
        /// Giao diện tạo đơn hàng mới
        /// </summary>
        public async Task<IActionResult> Create(ProductSearchInput input)
        {
            if (input == null)
            {
                input = new ProductSearchInput
                {
                    Page = 1,
                    PageSize = 5,
                    SearchValue = ""
                };
            }

            ViewBag.SearchValue = input.SearchValue;
            var result = await CatalogDataService.ListProductsAsync(input);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null)
                return NotFound();

            var item = new OrderDetailViewInfo
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo ?? string.Empty,
                Quantity = quantity,
                SalePrice = product.Price
            };

            ShopingCartService.AddToCart(item);
            return RedirectToAction("Create");
        }

        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            if (input == null)
            {
                input = new ProductSearchInput
                {
                    Page = 1,
                    PageSize = 5,
                    SearchValue = ""
                };
            }

            ViewBag.SearchValue = input.SearchValue;
            var result = await CatalogDataService.ListProductsAsync(input);
            return View(result);
        }
        /// <summary>
        /// Hiển thị giỏ hàng (các mặt hàng đã chọn để tạo đơn hàng mới hoặc các mặt hàng của đơn hàng đang xử lý)
        /// </summary>
        /// <returns></returns>
        public IActionResult showCart()
        {
            var cart = ShopingCartService.GetShopingCart();
            return View(cart);
        }
        /// <summary>
        /// Hiển thị chi tiết đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            var details = await SalesDataService.ListDetailsAsync(id);
            var model = new OrderDetailsViewModel
            {
                Order = order,
                Details = details
            };
            return View(model);
        }

        /// <summary>
        /// Chỉnh sửa mặt hàng trong giỏ
        /// </summary>
        /// <param name="id">Mã đơn hàng ( = 0 xử lý giỏ hàng , khác sử lý đơn hàng)</param>
        /// <param name="productId">Mã sản phẩm</param>
        public IActionResult EditCartItem(int id, int productId)
        {
            if (id == 0)
            {
                // Xử lý giỏ hàng
            }
            else
            {
                // Xử lý đơn hàng 
            }
            return View();
        }

        /// <summary>
        /// Xóa mặt hàng khỏi giỏ ra khỏi đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng ( = 0 xử lý giỏ hàng , khác sử lý đơn hàng)</param>
        /// <param name="productId">Mã sản phẩm</param>
        public IActionResult DeleteCartItem(int id, int productId)
        {
            if (id == 0)
            {
                ShopingCartService.RemoveFromCart(productId);
                return RedirectToAction("Create");
            }

            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public IActionResult ClearCart()
        {
            ShopingCartService.ClearCart();
            return RedirectToAction("Create");
        }

        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Accept(int id)
        {
            await SalesDataService.AcceptOrderAsync(id, 0);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Chuyển sang trạng thái đang giao
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Shipping(int id)
        {
            await SalesDataService.ShipOrderAsync(id, 0);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hoàn thành đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Finish(int id)
        {
            await SalesDataService.CompleteOrderAsync(id);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Reject(int id)
        {
            await SalesDataService.RejectOrderAsync(id, 0);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Cancel(int id)
        {
            await SalesDataService.CancelOrderAsync(id);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Delete(int id)
        {
            await SalesDataService.DeleteOrderAsync(id);
            return RedirectToAction("Index");
        }
    }
}
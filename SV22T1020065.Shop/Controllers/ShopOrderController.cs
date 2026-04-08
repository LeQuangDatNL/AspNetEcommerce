using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Models.Sales;
using SV22T1020065.Models.Partner;
using SV22T1020065.Models.Sales;
using SV22T1020065.Shop.AppCodes;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020065.Shop.Controllers
{
    /// <summary>
    /// Controller quản lý đơn hàng và giỏ hàng cho shop
    /// </summary>
    public class ShopOrderController : Controller
    {
        /// <summary>
        /// Hiển thị giỏ hàng
        /// </summary>
        public IActionResult Index()
        {
            var cart = ShoppingCartService.GetShoppingCart();
            return View(cart);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity <= 0)
                quantity = 1;

            // Lấy thông tin sản phẩm
            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            // Tạo item cho giỏ hàng
            var cartItem = new OrderDetailViewInfo
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo ?? "",
                Quantity = quantity,
                SalePrice = product.Price,
            };

            // Thêm vào giỏ hàng
            ShoppingCartService.AddToCart(cartItem);

            var totalItems = ShoppingCartService.GetShoppingCart().Sum(i => i.Quantity);
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng", totalItems });
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            int totalItems = 0; 

            if (quantity <= 0)
            {
                ShoppingCartService.RemoveFromCart(productId);
                totalItems = ShoppingCartService.GetShoppingCart().Sum(i => i.Quantity); // gán lại
                return Json(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng", totalItems });
            }

            var cartItem = ShoppingCartService.GetCartItem(productId);
            if (cartItem == null)
                return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng" });

            ShoppingCartService.UpdateCartItem(productId, quantity, (int)cartItem.SalePrice);
            totalItems = ShoppingCartService.GetShoppingCart().Sum(i => i.Quantity); // gán lại
            return Json(new { success = true, message = "Đã cập nhật số lượng", totalItems });
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            ShoppingCartService.RemoveFromCart(productId);
            var totalItems = ShoppingCartService.GetShoppingCart().Sum(i => i.Quantity);
            return Json(new { success = true, message = "Đã xóa sản phẩm khỏi giỏ hàng", totalItems });
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        [HttpPost]
        public IActionResult ClearCart()
        {
            ShoppingCartService.ClearCart();
            return Json(new { success = true, message = "Đã xóa toàn bộ giỏ hàng", totalItems = 0 });
        }

        private int? GetCurrentCustomerId()
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
                return null;

            if (!int.TryParse(userData.UserId, out int customerId))
                return null;

            return customerId;
        }

        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
                return null;

            return await PartnerDataService.GetCustomerAsync(customerId.Value);
        }

        /// <summary>
        /// Hiển thị trang thanh toán
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
                return RedirectToAction("Index");

            var customer = await GetCurrentCustomerAsync();
            ViewBag.Customer = customer;
            ViewBag.Provinces = await SelectListHelper.Provinces();

            return View(cart);
        }

        /// <summary>
        /// Xử lý thanh toán và tạo đơn hàng
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(string CustomerName, string ContactName, string Province, string Address, string Phone, string Email)
        {
            var customerId = GetCurrentCustomerId();
            if (!customerId.HasValue)
                return RedirectToAction("Login", "ShopAccount");

            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
                return RedirectToAction("Index");

            // Validate thông tin giao hàng
            if (string.IsNullOrWhiteSpace(Province) || string.IsNullOrWhiteSpace(Address))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin giao hàng";
                var customer = await GetCurrentCustomerAsync();
                ViewBag.Customer = customer;
                ViewBag.Provinces = await SelectListHelper.Provinces();
                return View(cart);
            }

            // Tạo đơn hàng
            var order = new Order
            {
                CustomerID = customerId.Value,
                DeliveryProvince = Province,
                DeliveryAddress = Address
            };

            int orderId = await SalesDataService.AddOrderAsync(order);
            if (orderId <= 0)
            {
                ViewBag.Error = "Không thể tạo đơn hàng, vui lòng thử lại";
                var customer = await GetCurrentCustomerAsync();
                ViewBag.Customer = customer;
                ViewBag.Provinces = await SelectListHelper.Provinces();
                return View(cart);
            }

            // Thêm chi tiết đơn hàng
            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                };

                bool result = await SalesDataService.AddDetailAsync(detail);
                if (!result)
                {
                    ViewBag.Error = "Có lỗi khi lưu chi tiết đơn hàng";
                    var customer = await GetCurrentCustomerAsync();
                    ViewBag.Customer = customer;
                    ViewBag.Provinces = await SelectListHelper.Provinces();
                    return View(cart);
                }
            }

            // Xóa giỏ hàng sau khi đặt hàng thành công
            ShoppingCartService.ClearCart();

            return RedirectToAction("OrderSuccess", new { orderId });
        }

        /// <summary>
        /// Hiển thị trang xác nhận đặt hàng thành công
        /// </summary>
        [Authorize]
        public IActionResult OrderSuccess(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        /// <summary>
        /// Hiển thị lịch sử đơn hàng của khách hàng
        /// </summary>
        [Authorize]
        public async Task<IActionResult> History(int page = 1, string status = "")
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
                return RedirectToAction("Login", "ShopAccount");

 
            int customerId = int.Parse(userData.UserId);
            Console.WriteLine($"Customer ID: {customerId}, Page: {page}, Status: {status}");
            var input = new OrderUserInput
            {
                Page = page,
                PageSize = 10,
                CustomerName = "",
                CustomerID = customerId,
            };

            var result = await SalesDataService.ListOrdersUserAsync(input);
            return View(result ?? new PagedResult<OrderViewInfo>());
        }

        /// <summary>
        /// Xem chi tiết đơn hàng
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
                return RedirectToAction("Login", "ShopAccount");

            if (!int.TryParse(userData.UserId, out int customerId))
                return RedirectToAction("Login", "ShopAccount");

            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != customerId)
                return NotFound();

            var details = await SalesDataService.ListDetailsAsync(id);
            ViewBag.Order = order;
            return View(details);
        }
    }
}
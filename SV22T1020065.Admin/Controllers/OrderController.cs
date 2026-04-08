using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    [Authorize(Roles = WebUserRoles.Sales + "," + WebUserRoles.Administrator)]
    public class OrderController : Controller
    {
        private int PAGESIZE = ApplicationContext.PAGE_SIZE;
        private const string OrderSearch = "OrderSearchProductInput";
        /// <summary>
        /// Hiển thị danh sách đơn hàng
        /// </summary>
        public async Task<IActionResult> Index(string customerName = "", OrderStatusEnum? status = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? orderID = null)
        {
            // Chuẩn bị input tìm kiếm
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(OrderSearch) ?? new OrderSearchInput
            {
                CustomerName = customerName,
                Status = status ?? OrderStatusEnum.New,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Page = 1,
                PageSize = PAGESIZE,
                SearchValue = orderID.HasValue ? orderID.Value.ToString() : ""
            };
            ViewBag.CustomerName = input.CustomerName;
            ViewBag.Status = input.Status;
            ViewBag.DateFrom = input.DateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = input.DateTo?.ToString("yyyy-MM-dd");
            // Lấy danh sách đơn hàng
            var result = await SalesDataService.ListOrdersAsync(input);

            return View(result);
        }

        /// <summary>
        /// Tìm kiếm đơn hàng
        /// </summary>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            var result = await SalesDataService.ListOrdersAsync(input);
            ApplicationContext.SetSessionData(OrderSearch, input);
            return View("Search", result);
        }

        /// <summary>
        /// Giao diện tạo đơn hàng mới
        /// </summary>
        public async Task<IActionResult> Create(ProductSearchInput input)
        {
            // Luôn set PageSize = 3, bỏ qua input
            if (input == null)
                input = new ProductSearchInput();

            input.Page = input.Page <= 0 ? 1 : input.Page;
            input.PageSize = 3;
            input.SearchValue = input.SearchValue ?? "";

            ViewBag.SearchValue = input.SearchValue;
            ViewBag.Customers = await GetCustomerSelectListAsync();
            ViewBag.Provinces = await SelectListHelper.Provinces();
            ViewBag.Cart = ShoppingCartService.GetShoppingCart();
            ViewBag.OrderStatus = OrderStatusEnum.New;

            var result = await CatalogDataService.ListProductsAsync(input);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int customerId, string deliveryProvince, string deliveryAddress)
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart == null || !cart.Any())
                ModelState.AddModelError("", "Giỏ hàng đang trống.");

            if (customerId <= 0)
                ModelState.AddModelError("customerId", "Vui lòng chọn khách hàng.");

            if (string.IsNullOrWhiteSpace(deliveryProvince))
                ModelState.AddModelError("deliveryProvince", "Vui lòng chọn tỉnh/thành.");

            if (string.IsNullOrWhiteSpace(deliveryAddress))
                ModelState.AddModelError("deliveryAddress", "Vui lòng nhập địa chỉ giao hàng.");

            if (!ModelState.IsValid)
            {
                ViewBag.SearchValue = "";
                ViewBag.Customers = await GetCustomerSelectListAsync();
                ViewBag.Provinces = await SelectListHelper.Provinces();
                ViewBag.Cart = cart;
                ViewBag.CustomerId = customerId;
                ViewBag.DeliveryProvince = deliveryProvince;
                ViewBag.DeliveryAddress = deliveryAddress;
                ViewBag.Error = "Vui lòng kiểm tra lại thông tin đơn hàng.";

                var result = await CatalogDataService.ListProductsAsync(new ProductSearchInput { Page = 1, PageSize = 3, SearchValue = "" });
                return View(result);
            }

            var order = new Order
            {
                CustomerID = customerId,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress
            };

            int orderId = await SalesDataService.AddOrderAsync(order);
            if (orderId <= 0)
            {
                ModelState.AddModelError("", "Không thể tạo đơn hàng.");
                ViewBag.SearchValue = "";
                ViewBag.Customers = await GetCustomerSelectListAsync();
                ViewBag.Provinces = await SelectListHelper.Provinces();
                ViewBag.Cart = cart;
                ViewBag.CustomerId = customerId;
                ViewBag.DeliveryProvince = deliveryProvince;
                ViewBag.DeliveryAddress = deliveryAddress;
                var result = await CatalogDataService.ListProductsAsync(new ProductSearchInput { Page = 1, PageSize = 3, SearchValue = "" });
                return View(result);
            }

            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                };

                await SalesDataService.AddDetailAsync(detail);
            }

            ShoppingCartService.ClearCart();
            return RedirectToAction("Detail", new { id = orderId });
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

            ShoppingCartService.AddToCart(item);
            return RedirectToAction("Create");
        }

        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            if (input == null)
                input = new ProductSearchInput();

            input.Page = input.Page <= 0 ? 1 : input.Page;
            input.PageSize = 3;
            input.SearchValue = input.SearchValue ?? "";

            ViewBag.SearchValue = input.SearchValue;
            var result = await CatalogDataService.ListProductsAsync(input);
            return PartialView(result);
        }
        /// <summary>
        /// Hiển thị giỏ hàng (các mặt hàng đã chọn để tạo đơn hàng mới hoặc các mặt hàng của đơn hàng đang xử lý)
        /// </summary>
        /// <returns></returns>
        public IActionResult showCart()
        {
            var cart = ShoppingCartService.GetShoppingCart();
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

            ViewBag.OrderStatus = order.Status;
            return View(model);
        }

        /// <summary>
        /// Chỉnh sửa mặt hàng trong giỏ
        /// </summary>
        /// <param name="id">Mã đơn hàng ( = 0 xử lý giỏ hàng , khác xử lý đơn hàng)</param>
        /// <param name="productId">Mã sản phẩm</param>
        public async Task<IActionResult> EditCartItem(int id, int productId)
        {
            if (id == 0)
            {
                var cartItem = ShoppingCartService.GetCartItem(productId);
                if (cartItem == null)
                    return NotFound();

                ViewBag.OrderId = id;
                return View(cartItem);
            }

            var orderItem = await SalesDataService.GetDetailAsync(id, productId);
            if (orderItem == null)
                return NotFound();

            ViewBag.OrderId = id;
            return View(orderItem);
        }

        [HttpPost]
        public async Task<IActionResult> EditCartItem(int id, int productId, int quantity)
        {
            if (quantity < 1)
                quantity = 1;

            if (id == 0)
            {
                var cartItem = ShoppingCartService.GetCartItem(productId);
                if (cartItem == null)
                    return NotFound();

                ShoppingCartService.UpdateCartItem(productId, quantity, (int)cartItem.SalePrice);
                return RedirectToAction("Create");
            }

            var orderItem = await SalesDataService.GetDetailAsync(id, productId);
            if (orderItem == null)
                return NotFound();

            var updated = new OrderDetail
            {
                OrderID = id,
                ProductID = productId,
                Quantity = quantity,
                SalePrice = orderItem.SalePrice
            };

            await SalesDataService.UpdateDetailAsync(updated);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Xóa mặt hàng khỏi giỏ ra khỏi đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng ( = 0 xử lý giỏ hàng , khác xử lý đơn hàng)</param>
        /// <param name="productId">Mã sản phẩm</param>
        public IActionResult DeleteCartItem(int id, int productId)
        {
            ViewBag.OrderId = id;
            ViewBag.ProductId = productId;
            return PartialView();
        }

        [HttpPost, ActionName("DeleteCartItem")]
        public async Task<IActionResult> DeleteCartItemConfirmed(int id, int productId)
        {
            if (id == 0)
            {
                ShoppingCartService.RemoveFromCart(productId);
                return RedirectToAction("Create");
            }

            await SalesDataService.DeleteDetailAsync(id, productId);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public IActionResult ClearCart()
        {
            return PartialView();
        }

        [HttpPost, ActionName("ClearCart")]
        public IActionResult ClearCartConfirmed()
        {
            ShoppingCartService.ClearCart();
            return RedirectToAction("Create");
        }

        private async Task<List<SelectListItem>> GetCustomerSelectListAsync()
        {
            var input = new PaginationSearchInput
            {
                Page = 1,
                PageSize = 50,
                SearchValue = ""
            };
            var customers = await PartnerDataService.ListCustomersAsync(input);
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Chọn khách hàng --" }
            };

            foreach (var customer in customers.DataItems)
            {
                list.Add(new SelectListItem
                {
                    Value = customer.CustomerID.ToString(),
                    Text = customer.CustomerName
                });
            }

            return list;
        }

        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        [HttpGet]
        public async Task<IActionResult> Accept(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            ViewBag.OrderId = id;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int id, IFormCollection form)
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrWhiteSpace(userData.UserId) || !int.TryParse(userData.UserId, out int employeeId))
            {
                TempData["Error"] = "Không thể xác định nhân viên hiện tại.";
                return RedirectToAction("Detail", new { id });
            }

            var result = await SalesDataService.AcceptOrderAsync(id, employeeId);
            Console.WriteLine($"AcceptOrderAsync result: {result}");
            if (!result)
            {
                TempData["Error"] = "Không thể duyệt đơn hàng. Vui lòng kiểm tra trạng thái.";
            }
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Chuyển sang trạng thái đang giao
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Shipping(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            // Kiểm tra phải duyệt trước mới được ship
            if (order.Status != OrderStatusEnum.Accepted)
            {
                TempData["Error"] = "Đơn hàng phải được duyệt trước khi chuyển giao.";
                return RedirectToAction("Detail", new { id });
            }

            var input = new PaginationSearchInput
            {
                Page = 1,
                PageSize = 50,
                SearchValue = ""
            };
            var shippers = await PartnerDataService.ListShippersAsync(input);

            ViewBag.OrderId = id;
            ViewBag.Shippers = shippers.DataItems;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperId)
        {
            if (shipperId <= 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn người giao hàng.");

                var order = await SalesDataService.GetOrderAsync(id);
                var input = new PaginationSearchInput
                {
                    Page = 1,
                    PageSize = 50,
                    SearchValue = ""
                };
                var shippers = await PartnerDataService.ListShippersAsync(input);

                ViewBag.OrderId = id;
                ViewBag.Shippers = shippers.DataItems;
                return View(order);
            }

            await SalesDataService.ShipOrderAsync(id, shipperId);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hoàn thành đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public async Task<IActionResult> Finish(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            // Kiểm tra phải ship trước mới được finish
            if (order.Status != OrderStatusEnum.Shipping)
            {
                TempData["Error"] = "Đơn hàng phải được chuyển giao trước khi hoàn thành.";
                return RedirectToAction("Detail", new { id });
            }

            ViewBag.OrderId = id;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int id, IFormCollection form)
        {
            await SalesDataService.CompleteOrderAsync(id);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();


            ViewBag.OrderId = id;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, IFormCollection form)
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrWhiteSpace(userData.UserId) || !int.TryParse(userData.UserId, out int employeeId))
            {
                TempData["Error"] = "Không thể xác định nhân viên hiện tại.";
                return RedirectToAction("Detail", new { id });
            }

            await SalesDataService.RejectOrderAsync(id, employeeId);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();


            ViewBag.OrderId = id;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, IFormCollection form)
        {
            await SalesDataService.CancelOrderAsync(id);
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status != OrderStatusEnum.Completed && order.Status != OrderStatusEnum.Cancelled)
            {
                TempData["Error"] = "Đơn hàng chỉ có thể bị xóa khi đã hoàn tất hoặc đã bị hủy.";
                return RedirectToAction("Detail", new { id });
            }

            ViewBag.OrderId = id;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, IFormCollection form)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status != OrderStatusEnum.Completed && order.Status != OrderStatusEnum.Cancelled)
            {
                TempData["Error"] = "Đơn hàng chỉ có thể bị xóa khi đã hoàn tất hoặc đã bị hủy.";
                return RedirectToAction("Detail", new { id });
            }

            await SalesDataService.DeleteOrderAsync(id);
            return RedirectToAction("Index");
        }
    }
}
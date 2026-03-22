using Microsoft.AspNetCore.Mvc;

namespace SV22T1020065.Admin.Controllers
{
    /// <summary>
    /// Các chức năng quản lý nghiệp vụ liên quan đến đơn hàng
    /// </summary>
    public class OrderController : Controller
    {
        /// <summary>
        /// Hiển thị danh sách đơn hàng
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Tìm kiếm đơn hàng
        /// </summary>
        public IActionResult Search(string keyword)
        {
            ViewBag.Keyword = keyword;
            return View("Index");
        }

        /// <summary>
        /// Giao diện tạo đơn hàng mới
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Hiển thị chi tiết đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public IActionResult Detail(int id)
        {
            ViewBag.OrderId = id;
            return View();
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
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        public IActionResult ClearCart()
        {
            return RedirectToAction("Create");
        }

        /// <summary>
        /// Duyệt đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public IActionResult Accept(int id)
        {
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Chuyển sang trạng thái đang giao
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public IActionResult Shipping(int id)
        {
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hoàn thành đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public IActionResult Finish(int id)
        {
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public IActionResult Reject(int id)
        {
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public IActionResult Cancel(int id)
        {
            return RedirectToAction("Detail", new { id });
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        public IActionResult Delete(int id)
        {
            return RedirectToAction("Index");
        }
    }
}
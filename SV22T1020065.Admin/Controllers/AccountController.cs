using Microsoft.AspNetCore.Mvc;

namespace SV22T1020065.Admin.Controllers
{
    /// <summary>
    /// Diều khiển các chức năng liên quan đến tài khoản người dùng, bao gồm đăng nhập, đăng xuất và thay đổi mật khẩu
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Đăng nhập vào hệ thống quản trị, xác thực thông tin người dùng và thiết lập phiên làm việc
        /// </summary>
        /// <returns></returns>
        public IActionResult login()
        {
            return View();
        }
        /// <summary>
        /// Đăng xuất khỏi hệ thống, xóa phiên làm việc và chuyển hướng về trang đăng nhập
        /// </summary>
        /// <returns></returns>
        public IActionResult logout()
        {
            return RedirectToAction("Login", "Account");
        }
        /// <summary>
        /// Thay đổi mật khẩu của người dùng đã đăng nhập, yêu cầu nhập mật khẩu cũ và mật khẩu mới để cập nhật thông tin tài khoản
        /// </summary>
        /// <returns></returns>
        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}

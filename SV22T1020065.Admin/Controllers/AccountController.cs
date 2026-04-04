using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020065.DataLayers.MySQL;
using SV22T1020065.Models.Security;
using System.Data;

namespace SV22T1020065.Admin.Controllers
{
    /// <summary>
    /// Diều khiển các chức năng liên quan đến tài khoản người dùng, bao gồm đăng nhập, đăng xuất và thay đổi mật khẩu
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string UserName,string Password)
        {
            ViewBag.UserName = UserName;

            if (string.IsNullOrEmpty(UserName))
                ModelState.AddModelError(nameof(UserName), "Tên đăng nhập không được để trống.");
            if (string.IsNullOrEmpty(Password))
                ModelState.AddModelError(nameof(Password), "Mật khẩu không được để trống.");
            if (!ModelState.IsValid)
                return View();

            string passswordHash = CryptHelper.HashMD5(Password);
            //TODU: Lấy thông tin tài khoản từ database
            // UserAccount = await AccountDataService.AuthenticateAsync(UserName, passswordHash);
            //lưu tạm
            var UserAccount = new UserAccount()
            {
                UserId = "1",
                UserName = UserName,
                DisplayName = "Nguyên Thị Thảo Mai",
                Email = UserName,
                Photo = "nophoto.png",
                RoleNames = "${WebUserRoles.Administrator},{WebUserRoles.DataManager}"
            };
            if (UserAccount == null)
            {
                ModelState.AddModelError("", "Đăng nhập thất bại");
                return View();
            }
            // Thông tin người dùng để hợp lệ

            // Chuẩn bị thông tin người dùng để lưu vào cookie
            var userData = new WebUserData()
            {
                UserId = UserAccount.UserId,
                UserName = UserAccount.UserName,
                DisplayName = UserAccount.DisplayName,
                Email = UserAccount.Email,
                Photo = UserAccount.Photo,
                Roles = UserAccount.RoleNames.Split(',').ToList()
            };
            // Tạo ra giấy chứng nhận (ClaimsPrincipal) dựa trên thông tin người dùng
            var principal = userData.CreatePrincipal();
            // Trao giấy chứng nhận cho ASP.NET Core để lưu vào cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear(); // Xóa session nếu có
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
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

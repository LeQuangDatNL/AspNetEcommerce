using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020065.BusinessLayers;
using SV22T1020065.DataLayers.Repository;
using SV22T1020065.Models.Security;
using SV22T1020065.Shop.AppCodes;
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

            string passwordHash = CryptHelper.HashMD5(Password);
            Console.WriteLine(passwordHash);
            //TODU: Lấy thông tin tài khoản từ database
            var UserAccount = await AccountDataService.LoginEmployeeAsync(UserName, passwordHash);
            //lưu tạm

            if (UserAccount == null)
            {
                ModelState.AddModelError("", "Đăng nhập thất bại");
                return View();
            }
            // Thông tin người dùng để hợp lệ

            // Chuẩn bị thông tin người dùng để lưu vào cookie
            Console.WriteLine(UserAccount.RoleNames);

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
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            ViewBag.OldPassword = OldPassword;
            ViewBag.NewPassword = NewPassword;
            ViewBag.ConfirmPassword = ConfirmPassword;
            if (string.IsNullOrWhiteSpace(OldPassword))
            {
                ViewBag.Error = "Mật khẩu cũ không được để trống";
                return View();
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ViewBag.Error = "Mật khẩu mới không được để trống";
                return View();
            }

            if (NewPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự";
                return View();
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ViewBag.Error = "Xác nhận mật khẩu mới không được để trống";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không đúng";
                return View();
            }

            // 2. Lấy email từ cookie
            var userData = User.GetUserData();
            string email = userData?.Email ?? "";
            Console.WriteLine(email);
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Không xác định được tài khoản";
                return View();
            }

            // 3. Kiểm tra mật khẩu cũ (hash trước khi so sánh)
            string oldHash = CryptHelper.HashMD5(OldPassword);
            var user = await AccountDataService.LoginEmployeeAsync(email, oldHash);
            if (user == null)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng";
                return View();
            }

            // 4. Cập nhật mật khẩu mới
            string newHash = CryptHelper.HashMD5(NewPassword);
            bool result = await AccountDataService.ChangeEmployeePasswordAsync(email, newHash);
            if (!result)
            {
                ViewBag.Error = "Đổi mật khẩu thất bại";
                return View();
            }

            // 5. Thành công
            ViewBag.OldPassword = null;
            ViewBag.NewPassword = null;
            ViewBag.ConfirmPassword = null;
            ViewBag.Success = "Đổi mật khẩu thành công";
            return View();
        }
    }
}

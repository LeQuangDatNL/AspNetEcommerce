using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SV22T1020065.BusinessLayers;
using SV22T1020065.DataLayers.Repository;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Models.Account;
using SV22T1020065.Models.Models.Sales;
using SV22T1020065.Models.Partner;
using SV22T1020065.Models.Sales;
using SV22T1020065.Models.Security;
using SV22T1020065.Shop.AppCodes;
using System.Data;

namespace SV22T1020065.Shop.Controllers
{
    /// <summary>
    /// Diều khiển các chức năng liên quan đến tài khoản người dùng, bao gồm đăng nhập, đăng xuất và thay đổi mật khẩu
    /// </summary>
    public class ShopAccountController : Controller
    {
        private async Task<List<SelectListItem>> GetProvinceSelectListAsync()
        {
            return await SelectListHelper.Provinces();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string UserName, string Password)
        {
            ViewBag.UserName = UserName;
            ViewBag.Error = null;
            if (string.IsNullOrEmpty(UserName))
                ModelState.AddModelError(nameof(UserName), "Tên đăng nhập không được để trống.");
            if (string.IsNullOrEmpty(Password))
                ModelState.AddModelError(nameof(Password), "Mật khẩu không được để trống.");
            if (!ModelState.IsValid)
                return View();

            string passwordHash = CryptHelper.HashMD5(Password);
            Console.WriteLine(passwordHash);
            //TODU: Lấy thông tin tài khoản từ database
            var UserAccount = await AccountDataService.LoginCustomerAsync(UserName, passwordHash);
            //lưu tạm

            if (UserAccount == null)
            {
                ViewBag.Error = "Đăng nhập thất bại";
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

            return RedirectToAction("Index", "ShopHome");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear(); // Xóa session nếu có
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "ShopAccount");
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
            var user = await AccountDataService.LoginCustomerAsync(email, oldHash);
            if (user == null)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng";
                return View();
            }

            // 4. Cập nhật mật khẩu mới
            string newHash = CryptHelper.HashMD5(NewPassword);
            bool result = await AccountDataService.ChangeCustomerPasswordAsync(email, newHash);
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
                return RedirectToAction("Login", "ShopAccount");

            if (!int.TryParse(userData.UserId, out int customerId))
                return RedirectToAction("Login", "ShopAccount");

            var model = await PartnerDataService.GetCustomerAsync(customerId);
            if (model == null)
                return RedirectToAction("Login", "ShopAccount");

            ViewBag.Provinces = await GetProvinceSelectListAsync();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(Customer model)
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
                return RedirectToAction("Login", "ShopAccount");

            if (!int.TryParse(userData.UserId, out int customerId))
                return RedirectToAction("Login", "ShopAccount");

            model.CustomerID = customerId;

            if (string.IsNullOrWhiteSpace(model.CustomerName))
            {
                TempData["Error"] = "Tên khách hàng không được để trống";
                return RedirectToAction("Profile");
            }

            if (string.IsNullOrWhiteSpace(model.ContactName))
            {
                TempData["Error"] = "Tên giao dịch không được để trống";
                return RedirectToAction("Profile");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                TempData["Error"] = "Email không được để trống";
                return RedirectToAction("Profile");
            }

            bool emailIsValid = await PartnerDataService.ValidatelCustomerEmailAsync(model.Email, customerId);
            if (!emailIsValid)
            {
                TempData["Error"] = "Email đã tồn tại, vui lòng chọn email khác";
                return RedirectToAction("Profile");
            }

            var currentCustomer = await PartnerDataService.GetCustomerAsync(customerId);
            if (currentCustomer == null)
                return RedirectToAction("Login", "ShopAccount");

            model.IsLocked = currentCustomer.IsLocked;
            bool result = await PartnerDataService.UpdateCustomerAsync(model);
            if (!result)
            {
                TempData["Error"] = "Cập nhật thông tin thất bại, vui lòng thử lại";
                return RedirectToAction("Profile");
            }

            var updatedUserData = new WebUserData()
            {
                UserId = userData.UserId,
                UserName = model.Email,
                DisplayName = model.CustomerName,
                Email = model.Email,
                Photo = userData.Photo,
                Roles = userData.Roles ?? new List<string>()
            };

            var principal = updatedUserData.CreatePrincipal();
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["Success"] = "Cập nhật thông tin cá nhân thành công";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Provinces = await GetProvinceSelectListAsync();
            return View(new AccountCustomer());
        }

        // POST: /ShopAccount/Register
        [HttpPost]
        public async Task<IActionResult> Register(AccountCustomer model, string Password, string ConfirmPassword)
        {
            // 1. Validate cơ bản
            if (string.IsNullOrWhiteSpace(model.CustomerName))
            {
                ViewBag.Error = "Tên khách hàng không được để trống";
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.ContactName))
            {
                ViewBag.Error = "Tên giao dịch không được để trống";
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ViewBag.Error = "Email không được để trống";
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự";
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Xác nhận mật khẩu không đúng";
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            // 2. Kiểm tra email đã tồn tại chưa
            var existing = await PartnerDataService.ValidatelCustomerEmailAsync(model.Email);
            if (!existing)
            {
                ViewBag.Error = "Email đã được đăng ký";
                ViewBag.Provinces = await GetProvinceSelectListAsync();
                return View(model);
            }

            // 3. Hash mật khẩu
            model.Password = CryptHelper.HashMD5(Password);
            model.IsLocked = false;

            // 4. Lưu tài khoản khách hàng
            int newCustomerId = await AccountDataService.RegisterCustomerAsync(model);
            if (newCustomerId <= 0)
            {
                ViewBag.Error = "Đăng ký thất bại, vui lòng thử lại";
                return View(model);
            }

            ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập";
            return RedirectToAction("Login", "ShopAccount");
        }

        /// <summary>
        /// Xem lịch sử mua hàng
        /// </summary>
        [Authorize]
        public async Task<IActionResult> OrderHistory(int page = 1)
        {
            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
                return RedirectToAction("Login", "ShopAccount");

            int customerId = int.Parse(userData.UserId);

            var input = new OrderUserInput
            {
                Page = page,
                PageSize = 10,
                SearchValue = "",
                CustomerID = customerId
            };

            // Lấy danh sách đơn hàng của khách hàng này
            var orders = await SalesDataService.ListOrdersAsync(input);
            


            return View(orders);
        }
    }
}

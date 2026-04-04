using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private int PAGESIZE = ApplicationContext.PAGE_SIZE;
        private static readonly string SESSION_KEY = "CustomerSearchInput";

        /// <summary>
        /// Giao diện chính
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SESSION_KEY)
                        ?? new PaginationSearchInput()
                        {
                            Page = page,
                            PageSize = PAGESIZE,
                            SearchValue = searchValue
                        };

            ViewBag.SearchValue = input.SearchValue;
            var result = await PartnerDataService.ListCustomersAsync(input);
            return View(result);
        }

        /// <summary>
        /// AJAX search + phân trang
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListCustomersAsync(input);

            ApplicationContext.SetSessionData(SESSION_KEY, input);

            return PartialView("Search", result);
        }
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng";
            var model = new Customer()
            {
                CustomerID = 0
            };

            return View("Edit", model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật khách hàng";
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null) 
                return View("Index");
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveData(Customer data)
        {
            try
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật khách hàng";

                // Load lại SelectList nếu trả về View do lỗi
                ViewBag.Provinces = await SelectListHelper.Provinces();

                // Validate
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                else if (await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID) == false)
                    ModelState.AddModelError(nameof(data.Email), "Email đã tồn tại trong hệ thống");

                if (string.IsNullOrEmpty(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Tỉnh/Thành phố không được để trống");

                // Gán mặc định
                data.ContactName = string.IsNullOrWhiteSpace(data.ContactName) ? data.CustomerName : data.ContactName;
                data.Address = string.IsNullOrWhiteSpace(data.Address) ? "N/A" : data.Address;
                data.Phone = string.IsNullOrWhiteSpace(data.Phone) ? "N/A" : data.Phone;

                if (!ModelState.IsValid)
                    return View("Edit", data);

                // Lưu DB
                if (data.CustomerID == 0)
                    await PartnerDataService.AddCustomerAsync(data);
                else
                    await PartnerDataService.UpdateCustomerAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View("Edit", data);
            }
        }
        // GET: Hiển thị thông tin khách hàng trước khi xóa
        public async Task<IActionResult> Delete(int id)
        {
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            // Kiểm tra xem khách hàng có thể xóa hay không
            ViewBag.CanDelete = !await PartnerDataService.IsUsedCustomerAsync(id);
            return View(model);
        }

        // POST: Thực hiện xóa khách hàng
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await PartnerDataService.IsUsedCustomerAsync(id))  
                return RedirectToAction("Index");
            await PartnerDataService.DeleteCustomerAsync(id);
            return RedirectToAction("Index");
        }
    }
}
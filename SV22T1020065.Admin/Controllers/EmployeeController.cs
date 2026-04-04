using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.HR;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private int PAGESIZE = ApplicationContext.PAGE_SIZE;
        private static readonly string SESSION_KEY = "EmployeeSearchInput";

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
            var result = await HRDataService.ListEmployeesAsync(input);
            return View(result);
        }

        // 🔥 FIX Ở ĐÂY
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await HRDataService.ListEmployeesAsync(input);

            ApplicationContext.SetSessionData(SESSION_KEY, input);

            return PartialView("Search", result); // ✅ đúng
        }

        // ===== CRUD giữ nguyên =====

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email nhân viên");
                else if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi nhân viên khác");

                
                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/employees", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";

                if (data.EmployeeID == 0)
                    await HRDataService.AddEmployeeAsync(data);
                else
                    await HRDataService.UpdateEmployeeAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại");
                return View("Edit", data);
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            var data = await HRDataService.GetEmployeeAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            bool isUsed = await HRDataService.IsUsedEmployeeAsync(id);
            ViewBag.CanDelete = !isUsed;
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 🔥 Kiểm tra có đang được sử dụng không
            bool isUsed = await HRDataService.IsUsedEmployeeAsync(id);

            // ❌ Nếu đang dùng → không cho xóa
            if (isUsed)
            {
                return RedirectToAction("Index");
            }

            // ✅ Xóa
            await HRDataService.DeleteEmployeeAsync(id);
            return RedirectToAction("Index");
        }
    }
}
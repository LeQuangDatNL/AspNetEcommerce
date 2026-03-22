using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.HR;

namespace SV22T1020065.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý nhân viên
    /// </summary>
    public class EmployeeController : Controller
    {
        private const int PAGESIZE = 10;

        /// <summary>
        /// Tìm kiếm và hiển thị danh sách nhân viên
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = PAGESIZE,
                SearchValue = searchValue ?? ""
            };

            var result = await HRDataService.ListEmployeesAsync(input);
            return View(result);
        }

        /// <summary>
        /// Giao diện bổ sung nhân viên
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var data = new Employee()
            {
                EmployeeID = 0,
                BirthDate = DateTime.Now.AddYears(-20), // Giá trị mặc định tránh lỗi null
                IsWorking = true
            };
            return View("Edit", data);
        }

        /// <summary>
        /// Giao diện cập nhật thông tin nhân viên
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var data = await HRDataService.GetEmployeeAsync(id);
            if (data == null) return RedirectToAction("Index");
            return View(data);
        }

        /// <summary>
        /// Xử lý lưu dữ liệu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Save(Employee data)
        {
            if (data.EmployeeID == 0)
                await HRDataService.AddEmployeeAsync(data);
            else
                await HRDataService.UpdateEmployeeAsync(data);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xác nhận xóa nhân viên
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var data = await HRDataService.GetEmployeeAsync(id);
            if (data == null) return RedirectToAction("Index");
            return View(data);
        }

        /// <summary>
        /// Thực hiện xóa nhân viên
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id, string confirm)
        {
            await HRDataService.DeleteEmployeeAsync(id);
            return RedirectToAction("Index");
        }
    }
}
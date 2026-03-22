using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Partner;

namespace SV22T1020065.Admin.Controllers
{
    public class SupplierController : Controller
    {
        private const int PAGESIZE = 5;

        /// <summary>
        /// danh sách nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = PAGESIZE,
                SearchValue = searchValue
            };

            var result = await PartnerDataService.ListSuppliersAsync(input);
            return View(result);
        }

        /// <summary>
        /// thêm mới nhà cung cấp
        /// </summary>
        /// <returns>trả về view tạo mới nhà cung cấp</returns>
        public IActionResult Create()
        {
            return View(new Supplier());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Supplier data)
        {
            if (!ModelState.IsValid)
                return View(data);

            await PartnerDataService.AddSupplierAsync(data);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// chỉnh sửa thông tin nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần chỉnh sửa</param>
        /// <returns>trả về view chỉnh sửa nhà cung cấp</returns>
        public async Task<IActionResult> Edit(int id)
        {
            var data = await PartnerDataService.GetSupplierAsync(id);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Supplier data)
        {
            if (!ModelState.IsValid)
                return View(data);

            await PartnerDataService.UpdateSupplierAsync(data);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// xóa nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            var data = await PartnerDataService.GetSupplierAsync(id);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Supplier data)
        {
            await PartnerDataService.DeleteSupplierAsync(data.SupplierID);
            return RedirectToAction("Index");
        }
    }
}
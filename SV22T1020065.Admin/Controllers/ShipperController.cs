using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Partner;

namespace SV22T1020065.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý nhà vận chuyển
    /// </summary>
    public class ShipperController : Controller
    {
        private const int PAGESIZE = 5;

        /// <summary>
        /// Tìm kiếm và hiển thị danh sách nhà vận chuyển
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
        /// Bổ sung chức năng tạo mới nhà vận chuyển
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View(new Shipper());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Shipper data)
        {
            if (!ModelState.IsValid)
                return View(data);

            await PartnerDataService.AddShipperAsync(data);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Cập nhật thông tin nhà vận chuyển
        /// </summary>
        /// <param name="id">Mã người vận chuyển</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            var data = await PartnerDataService.GetShipperAsync(id);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Shipper data)
        {
            if (!ModelState.IsValid)
                return View(data);

            await PartnerDataService.UpdateShipperAsync(data);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Xóa người vận chuyển
        /// </summary>
        /// <param name="id">Mã người vẫn chuyển</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            var data = await PartnerDataService.GetShipperAsync(id);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Shipper data)
        {
            await PartnerDataService.DeleteShipperAsync(data.ShipperID);
            return RedirectToAction("Index");
        }
    }
}
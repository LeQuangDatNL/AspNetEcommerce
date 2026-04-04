using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    [Authorize]
    /// <summary>
    /// Quản lý nhà vận chuyển
    /// </summary>
    public class ShipperController : Controller
    {
        private int PAGESIZE = ApplicationContext.PAGE_SIZE;
        private static readonly string SESSION_KEY = "ShipperSearchInput";

        /// <summary>
        /// Giao diện chính (AJAX)
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
            var result = await PartnerDataService.ListShippersAsync(input);
            return View(result);
        }

        /// <summary>
        /// AJAX search + phân trang
        /// </summary>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListShippersAsync(input); 

            ApplicationContext.SetSessionData(SESSION_KEY, input);

            return View("Search", result);
        }

        // ================= CRUD =================

        /// <summary>
        /// Tạo mới
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung người giao hàng";
            var model = new Shipper()
            {
                ShipperID = 0
            };

            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật người giao hàng";
            var model = await PartnerDataService.GetShipperAsync(id);
            if (model == null)
                return View("Index");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Shipper data)
        {
            try
            {
                ViewBag.Title = data.ShipperID == 0 ? "Bổ sung người giao hàng" : "Cập nhật người giao hàng";

                // Validate
                if (string.IsNullOrWhiteSpace(data.ShipperName))
                    ModelState.AddModelError(nameof(data.ShipperName), "Tên người giao hàng không được để trống");

                if (string.IsNullOrWhiteSpace(data.Phone))
                {
                    ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không được để trống");
                }
                else if (data.Phone.Length < 9)
                {
                    ModelState.AddModelError(nameof(data.Phone), "Số điện thoại phải có ít nhất 9 số");
                }


                // Nếu lỗi validate
                if (!ModelState.IsValid)
                    return View("Edit", data);

                // Lưu DB
                if (data.ShipperID == 0)
                    await PartnerDataService.AddShipperAsync(data);
                else
                    await PartnerDataService.UpdateShipperAsync(data);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xóa
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var data = await PartnerDataService.GetShipperAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        // POST: Xóa
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int shipperID)
        {
            await PartnerDataService.DeleteShipperAsync(shipperID);
            return RedirectToAction("Index");
        }
    }
}
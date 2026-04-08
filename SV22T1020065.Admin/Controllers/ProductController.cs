using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    [Authorize(Roles = WebUserRoles.Administrator + "," + WebUserRoles.DataManager + "," + WebUserRoles.Employee)]
    public class ProductController : Controller
    {
        private int PAGESIZE = ApplicationContext.PAGE_SIZE;
        private static readonly string SESSION_KEY = "ProductSearchInput";

        #region LIST + SEARCH

        public async Task<IActionResult> Index(int page = 1, string searchValue = "", int categoryID = 0, int supplierID = 0,
            decimal minPrice = 0, decimal maxPrice = 0)
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(SESSION_KEY)
                        ?? new ProductSearchInput()
                        {
                            Page = page,
                            PageSize = PAGESIZE,
                            SearchValue = searchValue ?? "",
                            CategoryID = categoryID,
                            SupplierID = supplierID,
                            MinPrice = minPrice,
                            MaxPrice = maxPrice
                        };

            ViewBag.SearchValue = input.SearchValue;
            ViewBag.CategoryID = input.CategoryID;
            ViewBag.SupplierID = input.SupplierID;
            ViewBag.MinPrice = input.MinPrice;
            ViewBag.MaxPrice = input.MaxPrice;
            Console.WriteLine($"Index - Page: {input.Page}, SearchValue: {input.SearchValue}, CategoryID: {input.CategoryID}, SupplierID: {input.SupplierID}, MinPrice: {input.MinPrice}, MaxPrice: {input.MaxPrice}");
            var result = await CatalogDataService.ListProductsAsync(input);
            return View(result);
        }

        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            Console.WriteLine($"Index - Page: {input.Page}, SearchValue: {input.SearchValue}, CategoryID: {input.CategoryID}, SupplierID: {input.SupplierID}, MinPrice: {input.MinPrice}, MaxPrice: {input.MaxPrice}");
            ApplicationContext.SetSessionData(SESSION_KEY, input);
            return PartialView("Search", result);
        }

        #endregion

        #region PRODUCT CRUD

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await SelectListHelper.Categories();
            ViewBag.Suppliers = await SelectListHelper.Suppliers();

            return View(new Product());
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = await SelectListHelper.Categories();
            ViewBag.Suppliers = await SelectListHelper.Suppliers();
            ViewBag.ProductId = id;
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product data, IFormFile Photo)
        {
            ViewBag.ProductId = id;

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await SelectListHelper.Categories();
                ViewBag.Suppliers = await SelectListHelper.Suppliers();
                ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
                ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
                return View(data);
            }

            // Xử lý upload ảnh nếu có
            if (Photo != null && Photo.Length > 0)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(data.Photo))
                {
                    DeleteUploadedFile(data.Photo);
                }
                // Lưu ảnh mới
                string fileName = await SaveUploadedFile(Photo);
                data.Photo = fileName;
            }

            // Cập nhật sản phẩm
            bool success = await CatalogDataService.UpdateProductAsync(data);
            if (!success)
            {
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
                ViewBag.Categories = await SelectListHelper.Categories();
                ViewBag.Suppliers = await SelectListHelper.Suppliers();
                ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
                ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
                return View(data);
            }

            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product data, IFormFile Photo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await SelectListHelper.Categories();
                ViewBag.Suppliers = await SelectListHelper.Suppliers();
                return View(data);
            }

            // Xử lý upload ảnh nếu có
            if (Photo != null && Photo.Length > 0)
            {
                string fileName = await SaveUploadedFile(Photo);
                data.Photo = fileName;
            }

            // Thêm sản phẩm mới
            int productId = await CatalogDataService.AddProductAsync(data);
            if (productId <= 0)
            {
                ModelState.AddModelError("", "Không thể thêm sản phẩm. Vui lòng thử lại.");
                ViewBag.Categories = await SelectListHelper.Categories();
                ViewBag.Suppliers = await SelectListHelper.Suppliers();
                return View(data);
            }

            return RedirectToAction("Index");
        }

        
        public async Task<IActionResult> Delete(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.CategoryName = (await SelectListHelper.Categories()).FirstOrDefault(c => c.Value == product.CategoryID.ToString())?.Text ?? "";
            ViewBag.SupplierName = (await SelectListHelper.Suppliers()).FirstOrDefault(s => s.Value == product.SupplierID.ToString())?.Text ?? "";
            
            // Kiểm tra xem product có đang được sử dụng không
            bool isUsed = await CatalogDataService.InUsedProductAsync(id);
            ViewBag.CanDelete = !isUsed;
            if (isUsed)
            {
                ViewBag.CannotDelete = true;
                ViewBag.ErrorMessage = "Sản phẩm đang được sử dụng trong đơn hàng. Không thể xóa.";
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, Product data)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return NotFound();

            // Kiểm tra xem product có đang được sử dụng không
            bool isUsed = await CatalogDataService.InUsedProductAsync(id);
            if (isUsed)
            {
                ViewBag.CategoryName = (await SelectListHelper.Categories()).FirstOrDefault(c => c.Value == product.CategoryID.ToString())?.Text ?? "";
                ViewBag.SupplierName = (await SelectListHelper.Suppliers()).FirstOrDefault(s => s.Value == product.SupplierID.ToString())?.Text ?? "";
                ViewBag.CannotDelete = true;
                ViewBag.ErrorMessage = "Sản phẩm đang được sử dụng trong đơn hàng. Không thể xóa.";
                return View(product);
            }

            // Xóa ảnh chính nếu có
            if (!string.IsNullOrEmpty(product.Photo))
            {
                DeleteUploadedFile(product.Photo);
            }

            // Xóa tất cả ảnh phụ
            var photos = await CatalogDataService.ListPhotosAsync(id);
            foreach (var photo in photos)
            {
                DeleteUploadedFile(photo.Photo);
            }

            // Xóa sản phẩm
            bool success = await CatalogDataService.DeleteProductAsync(id);
            if (!success)
            {
                ViewBag.CategoryName = (await SelectListHelper.Categories()).FirstOrDefault(c => c.Value == product.CategoryID.ToString())?.Text ?? "";
                ViewBag.SupplierName = (await SelectListHelper.Suppliers()).FirstOrDefault(s => s.Value == product.SupplierID.ToString())?.Text ?? "";
                ViewBag.CannotDelete = true;
                ViewBag.ErrorMessage = "Không thể xóa sản phẩm. Vui lòng thử lại.";
                return View(product);
            }

            return RedirectToAction("Index");
        }
        #endregion
        #region ATTRIBUTE

        public async Task<IActionResult> ListAttributes(int id)
        {
            ViewBag.ProductId = id;
            var attributes = await CatalogDataService.ListAttributesAsync(id);
            return View(attributes);
        }

        public IActionResult CreateAttribute(int id)
        {
            ViewBag.ProductId = id;
            return View(new ProductAttribute
            {
                ProductID = id,
                DisplayOrder = 1
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAttribute(int id, ProductAttribute data)
        {
            ViewBag.ProductId = id;

            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Tên không được để trống");

            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị không được để trống");

            if (!ModelState.IsValid)
                return View(data);

            data.ProductID = id;

            await CatalogDataService.AddAttributeAsync(data); // ✅ FIX

            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> EditAttribute(int id, long attributeId)
        {
            var attribute = await CatalogDataService.GetAttributeAsync(attributeId);
            if (attribute == null || attribute.ProductID != id)
                return NotFound();

            ViewBag.ProductId = id;
            return View(attribute);
        }

        [HttpPost]
        public async Task<IActionResult> EditAttribute(int id, long attributeId, ProductAttribute data)
        {
            ViewBag.ProductId = id;

            if (string.IsNullOrWhiteSpace(data.AttributeName))
                ModelState.AddModelError(nameof(data.AttributeName), "Tên không được để trống");

            if (string.IsNullOrWhiteSpace(data.AttributeValue))
                ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị không được để trống");

            if (!ModelState.IsValid)
                return View(data);

            data.AttributeID = attributeId;
            data.ProductID = id;

            await CatalogDataService.UpdateAttributeAsync(data); // ✅ FIX

            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> DeleteAttribute(int id, long attributeId)
        {
            await CatalogDataService.DeleteAttributeAsync(attributeId); // ✅ FIX
            TempData["Message"] = "Đã xóa thuộc tính thành công.";
            return RedirectToAction("Edit", new { id });
        }

        #endregion

        #region PHOTO

        public async Task<IActionResult> ListPhoto(int id)
        {
            ViewBag.ProductId = id;
            var photos = await CatalogDataService.ListPhotosAsync(id);
            return View(photos);
        }

        public IActionResult CreatePhoto(int id)
        {
            ViewBag.ProductId = id;
            return View(new ProductPhoto
            {
                ProductID = id,
                DisplayOrder = 1,
                IsHidden = false
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePhoto(int id, ProductPhoto data, IFormFile uploadPhoto)
        {
            ViewBag.ProductId = id;

            if (uploadPhoto == null || uploadPhoto.Length == 0)
                ModelState.AddModelError("uploadPhoto", "Vui lòng chọn ảnh");

            if (!ModelState.IsValid)
                return View(data);

            string fileName = await SaveUploadedFile(uploadPhoto);

            data.ProductID = id;
            data.Photo = fileName;

            await CatalogDataService.AddPhotoAsync(data); // ✅ FIX

            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> EditPhoto(int id, long photoId)
        {
            var photo = await CatalogDataService.GetPhotoAsync(photoId);
            if (photo == null || photo.ProductID != id)
                return NotFound();

            ViewBag.ProductId = id;
            return View(photo);
        }

        [HttpPost]
        public async Task<IActionResult> EditPhoto(int id, long photoId, ProductPhoto data, IFormFile uploadPhoto)
        {
            var photo = await CatalogDataService.GetPhotoAsync(photoId);
            if (photo == null || photo.ProductID != id)
                return NotFound();

            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                DeleteUploadedFile(photo.Photo);
                photo.Photo = await SaveUploadedFile(uploadPhoto);
            }

            photo.Description = data.Description ?? "";
            photo.DisplayOrder = data.DisplayOrder;
            photo.IsHidden = data.IsHidden;

            await CatalogDataService.UpdatePhotoAsync(photo); // ✅ FIX

            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> DeletePhoto(int id, long photoId)
        {
            var photo = await CatalogDataService.GetPhotoAsync(photoId);
            if (photo != null)
            {
                DeleteUploadedFile(photo.Photo);
                await CatalogDataService.DeletePhotoAsync(photoId); // ✅ FIX
                TempData["Message"] = "Đã xóa ảnh thành công.";
            }

            return RedirectToAction("Edit", new { id });
        }
        #endregion

        #region FILE

        private async Task<string> SaveUploadedFile(IFormFile file)
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        private void DeleteUploadedFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        #endregion
    }
}
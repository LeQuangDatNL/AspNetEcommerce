using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020065.BusinessLayers;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.Controllers
{
    [Authorize]
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

            var result = await CatalogDataService.ListProductsAsync(input);
            return View(result);
        }

        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(SESSION_KEY, input);
            return PartialView("Search", result);
        }

        #endregion

        #region PRODUCT CRUD

        public IActionResult Create()
        {
            return View();
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

        public IActionResult Delete(int id)
        {
            ViewBag.ProductId = id;
            return View();
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
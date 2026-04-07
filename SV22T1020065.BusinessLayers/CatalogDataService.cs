using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.DataLayers.Repository;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;

namespace SV22T1020065.BusinessLayers
{
    public static class CatalogDataService
    {
        private static readonly IGenericRepository<Category> categoryDB;
        private static readonly IProductRepository productDB; // Giả định bạn có Interface riêng cho Product

        static CatalogDataService()
        {
            categoryDB = new CategoryRepository(Configuration.ConnectionString);
            productDB = new ProductRepository(Configuration.ConnectionString);
        }
        // Các chức năng liên quan đến quản lý danh mục và mặt hàng
        #region CATEGORY (Loại hàng)
        public static async Task<PagedResult<Category>> ListCategoriesAsync(PaginationSearchInput input)
        {
            return await categoryDB.ListAsync(input);
        }

        public static async Task<Category?> GetCategoryAsync(int categoryID)
        {
            return await categoryDB.GetAsync(categoryID);
        }

        public static async Task<int> AddCategoryAsync(Category data)
        {
            return await categoryDB.AddAsync(data);
        }
        
        public static async Task<bool> UpdateCategoryAsync(Category data)
        {
            return await categoryDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteCategoryAsync(int categoryID)
        {
            if (await categoryDB.IsUsed(categoryID)) return false;
            return await categoryDB.DeleteAsync(categoryID);
        }
        #endregion

        #region PRODUCT (Mặt hàng)

        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
        /// </summary>
        public static async Task<PagedResult<Product>> ListProductsAsync(ProductSearchInput input)
        {
            return await productDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một mặt hàng
        /// </summary>
        public static async Task<Product?> GetProductAsync(int productID)
        {
            return await productDB.GetAsync(productID);
        }

        /// <summary>
        /// Bổ sung một mặt hàng mới
        /// </summary>
        public static async Task<int> AddProductAsync(Product data)
        {
            return await productDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin mặt hàng
        /// </summary>
        public static async Task<bool> UpdateProductAsync(Product data)
        {
            return await productDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa mặt hàng (nếu chưa có dữ liệu liên quan trong đơn hàng)
        /// </summary>
        public static async Task<bool> DeleteProductAsync(int productID)
        {
            if (await productDB.IsUsedAsync(productID))
                return false;
            return await productDB.DeleteAsync(productID);
        }

        /// <summary>
        /// Kiểm tra xem mặt hàng hiện có dữ liệu liên quan hay không
        /// </summary>
        public static async Task<bool> InUsedProductAsync(int productID)
        {
            return await productDB.IsUsedAsync(productID);
        }

        #endregion

        #region PRODUCT PHOTO (Ảnh mặt hàng)

        /// <summary>
        /// Lấy danh sách ảnh của mặt hàng
        /// </summary>
        public static async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            return await productDB.ListPhotosAsync(productID);
        }

        /// <summary>
        /// Lấy thông tin một ảnh
        /// </summary>
        public static async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            return await productDB.GetPhotoAsync(photoID);
        }

        /// <summary>
        /// Bổ sung ảnh cho mặt hàng
        /// </summary>
        public static async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            return await productDB.AddPhotoAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin ảnh
        /// </summary>
        public static async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            return await productDB.UpdatePhotoAsync(data);
        }

        /// <summary>
        /// Xóa ảnh của mặt hàng
        /// </summary>
        public static async Task<bool> DeletePhotoAsync(long photoID)
        {
            return await productDB.DeletePhotoAsync(photoID);
        }

        #endregion

        #region PRODUCT ATTRIBUTE (Thuộc tính mặt hàng)

        /// <summary>
        /// Lấy danh sách thuộc tính của mặt hàng
        /// </summary>
        public static async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            return await productDB.ListAttributesAsync(productID);
        }

        /// <summary>
        /// Lấy thông tin một thuộc tính
        /// </summary>
        public static async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            return await productDB.GetAttributeAsync(attributeID);
        }

        /// <summary>
        /// Bổ sung thuộc tính cho mặt hàng
        /// </summary>
        public static async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            return await productDB.AddAttributeAsync(data);
        }

        /// <summary>
        /// Cập nhật thuộc tính
        /// </summary>
        public static async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            return await productDB.UpdateAttributeAsync(data);
        }

        /// <summary>
        /// Xóa thuộc tính
        /// </summary>
        public static async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            return await productDB.DeleteAttributeAsync(attributeID);
        }

        #endregion
    }
}
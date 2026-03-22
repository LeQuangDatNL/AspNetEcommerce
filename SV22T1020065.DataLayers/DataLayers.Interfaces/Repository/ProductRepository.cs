using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;
using System.Data;

namespace SV22T1020065.DataLayers.SQLServer
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Product

        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = new PagedResult<Product>();

            // 1. Đếm tổng số dòng
            string sqlCount = @"SELECT COUNT(*) FROM Products
                                WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                                  AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                                  AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                                  AND (Price >= @MinPrice)
                                  AND (@MaxPrice <= 0 OR Price <= @MaxPrice)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
            {
                SearchValue = $"%{input.SearchValue}%",
                CategoryID = input.CategoryID,
                SupplierID = input.SupplierID,
                MinPrice = input.MinPrice,
                MaxPrice = input.MaxPrice
            });

            // 2. Lấy dữ liệu phân trang (Dùng cú pháp SQL Server OFFSET/FETCH)
            string sqlData = @"SELECT * FROM Products
                               WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
                                 AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                                 AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                                 AND (Price >= @MinPrice)
                                 AND (@MaxPrice <= 0 OR Price <= @MaxPrice)
                               ORDER BY ProductName
                               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Product>(sqlData, new
            {
                SearchValue = $"%{input.SearchValue}%",
                CategoryID = input.CategoryID,
                SupplierID = input.SupplierID,
                MinPrice = input.MinPrice,
                MaxPrice = input.MaxPrice,
                Offset = input.Offset,
                PageSize = input.PageSize
            });

            result.Page = input.Page;
            result.PageSize = input.PageSize;
            result.DataItems = data.ToList();

            return result;
        }

        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Products WHERE ProductID = @productID";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { productID });
        }

        public async Task<int> AddAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Products(ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                           VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE Products SET
                                ProductName=@ProductName,
                                ProductDescription=@ProductDescription,
                                SupplierID=@SupplierID,
                                CategoryID=@CategoryID,
                                Unit=@Unit,
                                Price=@Price,
                                Photo=@Photo,
                                IsSelling=@IsSelling
                           WHERE ProductID=@ProductID";
            return (await connection.ExecuteAsync(sql, data)) > 0;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM Products WHERE ProductID = @productID";
            return (await connection.ExecuteAsync(sql, new { productID })) > 0;
        }

        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT COUNT(*) FROM OrderDetails WHERE ProductID = @productID";
            int count = await connection.ExecuteScalarAsync<int>(sql, new { productID });
            return count > 0;
        }

        #endregion

        #region ProductAttribute (Thuộc tính)

        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM ProductAttributes WHERE ProductID = @productID ORDER BY DisplayOrder";
            return (await connection.QueryAsync<ProductAttribute>(sql, new { productID })).ToList();
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM ProductAttributes WHERE AttributeID = @attributeID";
            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO ProductAttributes(ProductID, AttributeName, AttributeValue, DisplayOrder)
                           VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                           SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE ProductAttributes SET
                                AttributeName=@AttributeName,
                                AttributeValue=@AttributeValue,
                                DisplayOrder=@DisplayOrder
                           WHERE AttributeID=@AttributeID";
            return (await connection.ExecuteAsync(sql, data)) > 0;
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM ProductAttributes WHERE AttributeID = @attributeID";
            return (await connection.ExecuteAsync(sql, new { attributeID })) > 0;
        }

        #endregion

        #region ProductPhoto (Ảnh thư viện)

        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM ProductPhotos WHERE ProductID = @productID ORDER BY DisplayOrder";
            return (await connection.QueryAsync<ProductPhoto>(sql, new { productID })).ToList();
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM ProductPhotos WHERE PhotoID = @photoID";
            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoID });
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO ProductPhotos(ProductID, PhotoPath, Description, DisplayOrder, IsHidden)
                           VALUES (@ProductID, @PhotoPath, @Description, @DisplayOrder, @IsHidden);
                           SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE ProductPhotos SET
                                PhotoPath=@PhotoPath,
                                Description=@Description,
                                DisplayOrder=@DisplayOrder,
                                IsHidden=@IsHidden
                           WHERE PhotoID=@PhotoID";
            return (await connection.ExecuteAsync(sql, data)) > 0;
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM ProductPhotos WHERE PhotoID = @photoID";
            return (await connection.ExecuteAsync(sql, new { photoID })) > 0;
        }

        #endregion
    }
}
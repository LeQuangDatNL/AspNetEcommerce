using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Catalog;
using SV22T1020065.Models.Common;

namespace SV22T1020065.DataLayers.SQLServer
{
    public class CategoryRepository : IGenericRepository<Category>
    {
        private readonly string _connectionString;

        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Categories(CategoryName, Description)
                           VALUES(@CategoryName, @Description);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE Categories
                           SET CategoryName = @CategoryName,
                               Description = @Description
                           WHERE CategoryID = @CategoryID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM Categories WHERE CategoryID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        public async Task<Category?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Categories WHERE CategoryID = @id";

            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { id });
        }

        public async Task<bool> IsUsed(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT COUNT(*)
                           FROM Products
                           WHERE CategoryID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }

        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = new PagedResult<Category>();

            // 1. Đếm tổng số dòng
            string sqlCount = @"SELECT COUNT(*)
                                FROM Categories
                                WHERE CategoryName LIKE @Search";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
            {
                Search = $"%{input.SearchValue}%"
            });

            // 2. Lấy dữ liệu phân trang
            string sqlData = @"SELECT *
                               FROM Categories
                               WHERE CategoryName LIKE @Search
                               ORDER BY CategoryName
                               OFFSET @Offset ROWS
                               FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Category>(sqlData, new
            {
                Search = $"%{input.SearchValue}%",
                Offset = input.Offset, // Sử dụng thuộc tính Offset có sẵn trong PaginationSearchInput
                PageSize = input.PageSize
            });

            result.Page = input.Page;
            result.PageSize = input.PageSize;
            result.DataItems = data.ToList();

            return result;
        }
    }
}
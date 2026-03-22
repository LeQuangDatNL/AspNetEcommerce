using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Partner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020065.DataLayers.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly string _connectionString;

        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region ListAsync

        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            var result = new PagedResult<Supplier>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            // Sửa bảng thành Suppliers
            string sqlCount = @"SELECT COUNT(*) 
                                FROM Suppliers 
                                WHERE (@SearchValue = N'') OR (SupplierName LIKE @SearchValue)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
            {
                SearchValue = $"%{input.SearchValue}%"
            });

            string sql = @"SELECT *
                           FROM Suppliers
                           WHERE (@SearchValue = N'') OR (SupplierName LIKE @SearchValue)
                           ORDER BY SupplierName
                           OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Supplier>(sql, new
            {
                SearchValue = $"%{input.SearchValue}%",
                Offset = (input.Page - 1) * input.PageSize,
                PageSize = input.PageSize
            });

            result.DataItems = data.ToList();

            return result;
        }

        #endregion

        #region GetAsync

        public async Task<Supplier?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = "SELECT * FROM Suppliers WHERE SupplierID = @Id";

            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { Id = id });
        }

        #endregion

        #region AddAsync

        public async Task<int> AddAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Suppliers 
                          (SupplierName, ContactName, Province, Address, Phone, Email)
                          VALUES 
                          (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
                          SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        #endregion

        #region UpdateAsync

        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Suppliers
                           SET SupplierName = @SupplierName,
                               ContactName = @ContactName,
                               Province = @Province,
                               Address = @Address,
                               Phone = @Phone,
                               Email = @Email
                           WHERE SupplierID = @SupplierID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        #endregion

        #region DeleteAsync

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = "DELETE FROM Suppliers WHERE SupplierID = @Id";

            int rows = await connection.ExecuteAsync(sql, new { Id = id });

            return rows > 0;
        }

        #endregion

        #region IsUsed

        public async Task<bool> IsUsed(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            // Kiểm tra xem nhà cung cấp có mặt trong bảng Products (Sản phẩm) không
            string sql = @"IF EXISTS (SELECT * FROM Products WHERE SupplierID = @Id)
                                SELECT 1
                           ELSE
                                SELECT 0";

            return await connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        #endregion

        #region ValidateEmailAsync

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);

            if (string.IsNullOrWhiteSpace(email))
                return false;

            string sql;

            if (id == 0)
            {
                sql = "SELECT COUNT(*) FROM Suppliers WHERE Email = @Email";
                int count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
                return count == 0;
            }
            else
            {
                sql = "SELECT COUNT(*) FROM Suppliers WHERE Email = @Email AND SupplierID <> @Id";
                int count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, Id = id });
                return count == 0;
            }
        }

        #endregion
    }
}
using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Partner;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020065.DataLayers.SQLServer
{
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);
            // Bổ sung các cột nếu Shipper của bạn có nhiều thông tin hơn
            string sql = @"INSERT INTO Shippers (ShipperName, Phone)
                           VALUES (@ShipperName, @Phone);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE Shippers
                           SET ShipperName = @ShipperName,
                               Phone = @Phone
                           WHERE ShipperID = @ShipperID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM Shippers WHERE ShipperID = @id";
            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Shippers WHERE ShipperID = @id";
            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { id });
        }

        public async Task<bool> IsUsed(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            // Kiểm tra xem Shipper có đang nằm trong đơn hàng nào không trước khi xóa
            string sql = @"IF EXISTS (SELECT * FROM Orders WHERE ShipperID = @id) 
                                SELECT 1 
                           ELSE 
                                SELECT 0";
            return await connection.ExecuteScalarAsync<bool>(sql, new { id });
        }

        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = new PagedResult<Shipper>();

            // 1. Đếm tổng số dòng (RowCount) để phân trang
            string sqlCount = @"SELECT COUNT(*)
                                FROM Shippers
                                WHERE (@SearchValue = N'') OR (ShipperName LIKE @SearchValue)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
            {
                SearchValue = $"%{input.SearchValue}%"
            });

            // 2. Lấy dữ liệu phân trang
            string sqlData = @"SELECT *
                               FROM Shippers
                               WHERE (@SearchValue = N'') OR (ShipperName LIKE @SearchValue)
                               ORDER BY ShipperName
                               OFFSET @Offset ROWS
                               FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Shipper>(sqlData, new
            {
                SearchValue = $"%{input.SearchValue}%",
                Offset = (input.Page - 1) * input.PageSize,
                PageSize = input.PageSize
            });

            result.Page = input.Page;
            result.PageSize = input.PageSize;
            result.DataItems = data.ToList();

            return result;
        }
    }
}
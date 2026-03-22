using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Sales;

namespace SV22T1020065.DataLayers.SQLServer
{
    /// <summary>
    /// Triển khai các chức năng xử lý dữ liệu cho đơn hàng trên SQL Server
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = new PagedResult<OrderViewInfo>();

            // 1. Đếm tổng số dòng (RowCount)
            string sqlCount = @"SELECT COUNT(*) 
                                FROM View_Orders
                                WHERE (@Status = 0 OR Status = @Status)
                                  AND (@FromTime IS NULL OR OrderTime >= @FromTime)
                                  AND (@ToTime IS NULL OR OrderTime <= @ToTime)
                                  AND (CustomerName LIKE @Search OR ShipperName LIKE @Search)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
            {
                Status = input.Status,
                FromTime = "",
                ToTime = "",
                Search = $"%{input.SearchValue}%"
            });

            // 2. Lấy dữ liệu phân trang (DataItems)
            string sqlData = @"SELECT *
                               FROM View_Orders
                               WHERE (@Status = 0 OR Status = @Status)
                                 AND (@FromTime IS NULL OR OrderTime >= @FromTime)
                                 AND (@ToTime IS NULL OR OrderTime <= @ToTime)
                                 AND (CustomerName LIKE @Search OR ShipperName LIKE @Search)
                               ORDER BY OrderID DESC
                               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<OrderViewInfo>(sqlData, new
            {
                Status = input.Status,
                FromTime = "",
                ToTime = "",
                Search = $"%{input.SearchValue}%",
                Offset = input.Offset, // Sử dụng thuộc tính Offset có sẵn trong PaginationSearchInput
                PageSize = input.PageSize
            });

            result.Page = input.Page;
            result.PageSize = input.PageSize;
            result.DataItems = data.ToList();

            return result;
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM View_Orders WHERE OrderID = @orderID";
            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
        }

        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Orders(OrderTime, Status, CustomerID, EmployeeID, ShipperID, ShippedTime, FinishedTime)
                           VALUES(@OrderTime, @Status, @CustomerID, @EmployeeID, @ShipperID, @ShippedTime, @FinishedTime);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE Orders 
                           SET Status = @Status, 
                               ShipperID = @ShipperID, 
                               ShippedTime = @ShippedTime, 
                               FinishedTime = @FinishedTime
                           WHERE OrderID = @OrderID";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            // Xóa chi tiết trước để bảo đảm toàn vẹn dữ liệu
            await connection.ExecuteAsync("DELETE FROM OrderDetails WHERE OrderID = @orderID", new { orderID });
            string sql = "DELETE FROM Orders WHERE OrderID = @orderID";
            int rows = await connection.ExecuteAsync(sql, new { orderID });
            return rows > 0;
        }

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM View_OrderDetails WHERE OrderID = @orderID";
            var data = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { orderID });
            return data.ToList();
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM View_OrderDetails WHERE OrderID = @orderID AND ProductID = @productID";
            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql, new { orderID, productID });
        }

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO OrderDetails(OrderID, ProductID, Quantity, SalePrice)
                           VALUES(@OrderID, @ProductID, @Quantity, @SalePrice)";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE OrderDetails 
                           SET Quantity = @Quantity, SalePrice = @SalePrice 
                           WHERE OrderID = @OrderID AND ProductID = @ProductID";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = "DELETE FROM OrderDetails WHERE OrderID = @orderID AND ProductID = @productID";
            int rows = await connection.ExecuteAsync(sql, new { orderID, productID });
            return rows > 0;
        }
    }
}
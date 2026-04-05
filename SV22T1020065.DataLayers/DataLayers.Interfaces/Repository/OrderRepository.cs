using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Sales;

namespace SV22T1020065.DataLayers.SQLServer
{
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

            var searchText = string.IsNullOrWhiteSpace(input.SearchValue) ? "" : input.SearchValue;

            // 🔹 COUNT
            string sqlCount = @"SELECT COUNT(*)
                                FROM v_OrderList
                                WHERE (@Status = 0 OR Status = @Status)
                                  AND (@FromTime IS NULL OR OrderDate >= @FromTime)
                                  AND (@ToTime IS NULL OR OrderDate <= @ToTime)
                                  AND (CustomerName LIKE @Search OR CustomerPhone LIKE @Search)"; // ✅ FIX

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
            {
                Status = input.Status,
                FromTime = input.DateFrom,
                ToTime = input.DateTo,
                Search = $"%{searchText}%"
            });

            // 🔹 DATA
            string sqlData = @"SELECT *
                               FROM v_OrderList
                               WHERE (@Status = 0 OR Status = @Status)
                                 AND (@FromTime IS NULL OR OrderDate >= @FromTime)
                                 AND (@ToTime IS NULL OR OrderDate <= @ToTime)
                                 AND (CustomerName LIKE @Search OR CustomerPhone LIKE @Search) -- ✅ FIX
                               ORDER BY OrderID DESC
                               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<OrderViewInfo>(sqlData, new
            {
                Status = input.Status,
                FromTime = input.DateFrom,
                ToTime = input.DateTo,
                Search = $"%{searchText}%",
                Offset = input.Offset,
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

            string sql = @"SELECT *
                           FROM v_OrderList
                           WHERE OrderID = @orderID";

            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
        }

        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Orders(OrderTime, Status, CustomerID, EmployeeID, ShipperID, ShippedTime, FinishedTime, DeliveryProvince, DeliveryAddress)
                           VALUES(@OrderTime, @Status, @CustomerID, @EmployeeID, @ShipperID, @ShippedTime, @FinishedTime, @DeliveryProvince, @DeliveryAddress);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"UPDATE Orders 
                           SET Status = @Status,
                               DeliveryProvince = @DeliveryProvince,
                               DeliveryAddress = @DeliveryAddress,
                               CustomerID = @CustomerID,
                               EmployeeID = @EmployeeID,
                               AcceptTime = @AcceptTime,
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

            await connection.ExecuteAsync("DELETE FROM OrderDetails WHERE OrderID = @orderID", new { orderID });

            string sql = "DELETE FROM Orders WHERE OrderID = @orderID";
            int rows = await connection.ExecuteAsync(sql, new { orderID });

            return rows > 0;
        }

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT od.OrderID,
                                  od.ProductID,
                                  od.Quantity,
                                  od.SalePrice,
                                  p.ProductName,
                                  p.Unit,
                                  p.Photo
                           FROM OrderDetails od
                           LEFT JOIN Products p ON od.ProductID = p.ProductID
                           WHERE od.OrderID = @orderID";

            var data = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { orderID });
            return data.ToList();
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT od.OrderID,
                                  od.ProductID,
                                  od.Quantity,
                                  od.SalePrice,
                                  p.ProductName,
                                  p.Unit,
                                  p.Photo
                           FROM OrderDetails od
                           LEFT JOIN Products p ON od.ProductID = p.ProductID
                           WHERE od.OrderID = @orderID AND od.ProductID = @productID";

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
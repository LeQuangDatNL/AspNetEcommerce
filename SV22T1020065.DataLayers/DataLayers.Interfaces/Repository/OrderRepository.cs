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
                                FROM Orders o
                                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                                LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                                WHERE (@Status = 0 OR o.Status = @Status)
                                  AND (@FromTime IS NULL OR o.OrderTime >= @FromTime)
                                  AND (@ToTime IS NULL OR o.OrderTime <= @ToTime)
                                  AND (c.CustomerName LIKE @Search OR s.ShipperName LIKE @Search)";

            var searchText = string.IsNullOrWhiteSpace(input.SearchValue) ? input.CustomerName : input.SearchValue;
            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
            {
                Status = input.Status,
                FromTime = input.DateFrom,
                ToTime = input.DateTo,
                Search = $"%{searchText}%"
            });

            // 2. Lấy dữ liệu phân trang (DataItems)
            string sqlData = @"SELECT o.OrderID,
                                     o.CustomerID,
                                     o.OrderTime,
                                     o.DeliveryProvince,
                                     o.DeliveryAddress,
                                     o.EmployeeID,
                                     o.AcceptTime,
                                     o.ShipperID,
                                     o.ShippedTime,
                                     o.FinishedTime,
                                     o.Status,
                                     c.CustomerName,
                                     c.ContactName AS CustomerContactName,
                                     c.Email AS CustomerEmail,
                                     c.Phone AS CustomerPhone,
                                     c.Address AS CustomerAddress,
                                     e.FullName AS EmployeeName,
                                     sh.ShipperName,
                                     sh.Phone AS ShipperPhone,
                                     COALESCE(od.TotalPrice, 0) AS TotalPrice
                               FROM Orders o
                               LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                               LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                               LEFT JOIN Shippers sh ON o.ShipperID = sh.ShipperID
                               LEFT JOIN (
                                    SELECT OrderID, SUM(Quantity * SalePrice) AS TotalPrice
                                    FROM OrderDetails
                                    GROUP BY OrderID
                               ) od ON od.OrderID = o.OrderID
                               WHERE (@Status = 0 OR o.Status = @Status)
                                 AND (@FromTime IS NULL OR o.OrderTime >= @FromTime)
                                 AND (@ToTime IS NULL OR o.OrderTime <= @ToTime)
                                 AND (c.CustomerName LIKE @Search OR sh.ShipperName LIKE @Search)
                               ORDER BY o.OrderID DESC
                               OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<OrderViewInfo>(sqlData, new
            {
                Status = input.Status,
                FromTime = input.DateFrom,
                ToTime = input.DateTo,
                Search = $"%{searchText}%",
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
            string sql = @"SELECT o.OrderID,
                                  o.CustomerID,
                                  o.OrderTime,
                                  o.DeliveryProvince,
                                  o.DeliveryAddress,
                                  o.EmployeeID,
                                  o.AcceptTime,
                                  o.ShipperID,
                                  o.ShippedTime,
                                  o.FinishedTime,
                                  o.Status,
                                  c.CustomerName,
                                  c.ContactName AS CustomerContactName,
                                  c.Email AS CustomerEmail,
                                  c.Phone AS CustomerPhone,
                                  c.Address AS CustomerAddress,
                                  e.FullName AS EmployeeName,
                                  sh.ShipperName,
                                  sh.Phone AS ShipperPhone,
                                  COALESCE(od.TotalPrice, 0) AS TotalPrice
                           FROM Orders o
                           LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                           LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                           LEFT JOIN Shippers sh ON o.ShipperID = sh.ShipperID
                           LEFT JOIN (
                                SELECT OrderID, SUM(Quantity * SalePrice) AS TotalPrice
                                FROM OrderDetails
                                GROUP BY OrderID
                           ) od ON od.OrderID = o.OrderID
                           WHERE o.OrderID = @orderID";
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
            // Xóa chi tiết trước để bảo đảm toàn vẹn dữ liệu
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
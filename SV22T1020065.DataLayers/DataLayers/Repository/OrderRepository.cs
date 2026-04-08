using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Models.Sales;
using SV22T1020065.Models.Sales;

namespace SV22T1020065.DataLayers.Repository;

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

        var searchText = string.IsNullOrWhiteSpace(input.CustomerName) ? "" : input.CustomerName;

        // 🔹 COUNT
        string sqlCount = @"SELECT COUNT(*)
                                FROM dbo.Orders AS o
                                LEFT JOIN dbo.Customers AS c ON o.CustomerID = c.CustomerID
                                LEFT JOIN dbo.Employees AS e ON o.EmployeeID = e.EmployeeID
                                LEFT JOIN dbo.Shippers AS sh ON o.ShipperID = sh.ShipperID
                                WHERE (@Status = 0 OR o.Status = @Status)
                                  AND (@FromTime IS NULL OR o.OrderTime >= @FromTime)
                                  AND (@ToTime IS NULL OR o.OrderTime <= @ToTime)
                                  AND (c.CustomerName LIKE @Search OR c.Phone LIKE @Search)";

        result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
        {
            Status = input.Status,
            FromTime = input.DateFrom,
            ToTime = input.DateTo,
            Search = $"%{searchText}%"
        });

        // 🔹 DATA
        string sqlData = @"SELECT o.OrderID,
                                  o.CustomerID,
                                  o.OrderTime AS OrderTime,
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
                               FROM dbo.Orders AS o
                               LEFT JOIN dbo.Customers AS c ON o.CustomerID = c.CustomerID
                               LEFT JOIN dbo.Employees AS e ON o.EmployeeID = e.EmployeeID
                               LEFT JOIN dbo.Shippers AS sh ON o.ShipperID = sh.ShipperID
                               LEFT JOIN (
                                   SELECT OrderID, SUM(Quantity * SalePrice) AS TotalPrice
                                   FROM dbo.OrderDetails
                                   GROUP BY OrderID
                               ) AS od ON od.OrderID = o.OrderID
                               WHERE (@Status = 0 OR o.Status = @Status)
                                 AND (@FromTime IS NULL OR o.OrderTime >= @FromTime)
                                 AND (@ToTime IS NULL OR o.OrderTime <= @ToTime)
                                 AND (c.CustomerName LIKE @Search OR c.Phone LIKE @Search)
                               ORDER BY o.OrderID DESC
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
    public async Task<PagedResult<OrderViewInfo>> ListOrdersByUserAsync(OrderUserInput input)
    {
        using var connection = new SqlConnection(_connectionString);
        var result = new PagedResult<OrderViewInfo>();

        var searchText = string.IsNullOrWhiteSpace(input.CustomerName) ? "" : input.CustomerName;

        string sqlCount = @"
        SELECT COUNT(*)
        FROM dbo.Orders AS o
        LEFT JOIN dbo.Customers AS c ON o.CustomerID = c.CustomerID
        LEFT JOIN dbo.Employees AS e ON o.EmployeeID = e.EmployeeID
        LEFT JOIN dbo.Shippers AS sh ON o.ShipperID = sh.ShipperID
        WHERE (@Status = 0 OR o.Status = @Status)
          AND (@FromTime IS NULL OR o.OrderTime >= @FromTime)
          AND (@ToTime IS NULL OR o.OrderTime <= @ToTime)
          AND (@CustomerID IS NULL OR o.CustomerID = @CustomerID)
          AND (c.CustomerName LIKE @Search OR c.Phone LIKE @Search)
    ";

        string sqlData = @"
        SELECT o.OrderID,
               o.CustomerID,
               o.OrderTime AS OrderTime,
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
        FROM dbo.Orders AS o
        LEFT JOIN dbo.Customers AS c ON o.CustomerID = c.CustomerID
        LEFT JOIN dbo.Employees AS e ON o.EmployeeID = e.EmployeeID
        LEFT JOIN dbo.Shippers AS sh ON o.ShipperID = sh.ShipperID
        LEFT JOIN (
            SELECT OrderID, SUM(Quantity * SalePrice) AS TotalPrice
            FROM dbo.OrderDetails
            GROUP BY OrderID
        ) AS od ON od.OrderID = o.OrderID
        WHERE (@Status = 0 OR o.Status = @Status)
          AND (@FromTime IS NULL OR o.OrderTime >= @FromTime)
          AND (@ToTime IS NULL OR o.OrderTime <= @ToTime)
          AND (@CustomerID IS NULL OR o.CustomerID = @CustomerID)
          AND (c.CustomerName LIKE @Search OR c.Phone LIKE @Search)
        ORDER BY o.OrderID DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    ";

        // 🔹 Parameters dùng cho cả COUNT & DATA
        var parameters = new
        {
            Status = input.Status,
            FromTime = input.DateFrom,
            ToTime = input.DateTo,
            CustomerID = input.CustomerID, // 🔹 CustomerID nullable
            Search = $"%{searchText}%",
            Offset = input.Offset,
            PageSize = input.PageSize
        };

        result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, parameters);
        var data = await connection.QueryAsync<OrderViewInfo>(sqlData, parameters);

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
                              o.OrderTime AS OrderTime,
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
                       FROM dbo.Orders AS o
                       LEFT JOIN dbo.Customers AS c ON o.CustomerID = c.CustomerID
                       LEFT JOIN dbo.Employees AS e ON o.EmployeeID = e.EmployeeID
                       LEFT JOIN dbo.Shippers AS sh ON o.ShipperID = sh.ShipperID
                       LEFT JOIN (
                           SELECT OrderID, SUM(Quantity * SalePrice) AS TotalPrice
                           FROM dbo.OrderDetails
                           GROUP BY OrderID
                       ) AS od ON od.OrderID = o.OrderID
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
        Console.WriteLine($"Updating OrderID: {data.OrderID}, Status: {data.Status}, CustomerID: {data.CustomerID}, EmployeeID: {data.EmployeeID}, ShipperID: {data.ShipperID}");
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
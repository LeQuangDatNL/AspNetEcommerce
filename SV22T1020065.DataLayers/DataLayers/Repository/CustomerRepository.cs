using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.Partner;

namespace SV22T1020065.DataLayers.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> AddAsync(Customer data)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = @"INSERT INTO Customers
                          (CustomerName, ContactName, Province, Address, Phone, Email, IsLocked)
                          VALUES
                          (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await connection.ExecuteScalarAsync<int>(sql, data);
    }

    public async Task<bool> UpdateAsync(Customer data)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = @"UPDATE Customers
                           SET CustomerName=@CustomerName,
                               ContactName=@ContactName,
                               Province=@Province,
                               Address=@Address,
                               Phone=@Phone,
                               Email=@Email,
                               IsLocked=@IsLocked
                           WHERE CustomerID=@CustomerID";

        int rows = await connection.ExecuteAsync(sql, data);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = "DELETE FROM Customers WHERE CustomerID=@id";

        int rows = await connection.ExecuteAsync(sql, new { id });
        return rows > 0;
    }

    public async Task<Customer?> GetAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = "SELECT * FROM Customers WHERE CustomerID=@id";

        return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
    }

    public async Task<bool> IsUsed(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE CustomerID=@id";

        int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

        return count > 0;
    }

    public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = new PagedResult<Customer>();

        string sqlCount = @"SELECT COUNT(*)
                                FROM Customers
                                WHERE CustomerName LIKE @Search";

        result.RowCount = await connection.ExecuteScalarAsync<int>(
            sqlCount,
            new { Search = $"%{input.SearchValue}%" }
        );

        string sql = @"SELECT *
                           FROM Customers
                           WHERE CustomerName LIKE @Search
                           ORDER BY CustomerName
                           OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await connection.QueryAsync<Customer>(
            sql,
            new
            {
                Search = $"%{input.SearchValue}%",
                Offset = (input.Page - 1) * input.PageSize,
                PageSize = input.PageSize
            });

        result.Page = input.Page;
        result.PageSize = input.PageSize;
        result.DataItems = data.ToList();

        return result;
    }

    public async Task<bool> ValidateEmailAsync(string email, int id = 0)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = @"SELECT COUNT(*)
                           FROM Customers
                           WHERE Email=@email AND CustomerID<>@id";

        int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });

        return count == 0;
    }
}
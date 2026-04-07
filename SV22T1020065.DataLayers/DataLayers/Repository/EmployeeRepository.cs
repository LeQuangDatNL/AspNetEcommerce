using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Common;
using SV22T1020065.Models.HR;

namespace SV22T1020065.DataLayers.Repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly string _connectionString;

    public EmployeeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> AddAsync(Employee data)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = @"INSERT INTO Employees
                          (FullName, BirthDate, Address, Phone, Email, Photo, IsWorking)
                          VALUES
                          (@FullName, @BirthDate, @Address, @Phone, @Email, @Photo, @IsWorking);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await connection.ExecuteScalarAsync<int>(sql, data);
    }

    public async Task<bool> UpdateAsync(Employee data)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = @"UPDATE Employees
                           SET FullName=@FullName,
                               BirthDate=@BirthDate,
                               Address=@Address,
                               Phone=@Phone,
                               Email=@Email,
                               Photo=@Photo,
                               IsWorking=@IsWorking
                           WHERE EmployeeID=@EmployeeID";

        int rows = await connection.ExecuteAsync(sql, data);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = "DELETE FROM Employees WHERE EmployeeID=@id";

        int rows = await connection.ExecuteAsync(sql, new { id });
        return rows > 0;
    }

    public async Task<Employee?> GetAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = "SELECT * FROM Employees WHERE EmployeeID=@id";

        return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
    }

    public async Task<bool> IsUsed(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = @"SELECT COUNT(*) 
                           FROM Orders 
                           WHERE EmployeeID=@id";

        int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

        return count > 0;
    }

    public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
    {
        using var connection = new SqlConnection(_connectionString);

        var result = new PagedResult<Employee>();

        string sqlCount = @"SELECT COUNT(*) 
                        FROM Employees
                        WHERE FullName LIKE @Search";

        result.RowCount = await connection.ExecuteScalarAsync<int>(
            sqlCount,
            new { Search = $"%{input.SearchValue}%" }
        );

        string sql = @"SELECT *
                   FROM Employees
                   WHERE FullName LIKE @Search
                   ORDER BY FullName
                   OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await connection.QueryAsync<Employee>(
            sql,
            new
            {
                Search = $"%{input.SearchValue}%",
                Offset = (input.Page - 1) * input.PageSize,
                PageSize = input.PageSize
            });

        // Gán thông tin phân trang
        result.Page = input.Page;
        result.PageSize = input.PageSize;

        // Sửa lỗi ở đây
        result.DataItems = data.ToList();

        return result;
    }
    public async Task<bool> ValidateEmailAsync(string email, int id = 0)
    {
        using var connection = new SqlConnection(_connectionString);

        string sql = @"SELECT COUNT(*)
                           FROM Employees
                           WHERE Email=@email AND EmployeeID<>@id";

        int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });

        return count == 0;
    }
}
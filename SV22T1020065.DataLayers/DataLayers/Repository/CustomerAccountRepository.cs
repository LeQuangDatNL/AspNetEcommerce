using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Models.Account;
using SV22T1020065.Models.Security;

namespace SV22T1020065.DataLayers.Repository;

public class CustomerAccountRepository : IUserAccountRepository
{
    private readonly string _connectionString;

    public CustomerAccountRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT 
                                    CustomerID     AS UserId,
                                    Email          AS UserName,
                                    CustomerName   AS DisplayName,
                                    Email          AS Email
                               FROM Customers
                               WHERE Email = @userName AND Password = @password";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql,
                new { userName, password });
        }
    }

    public async Task<bool> ChangePassword(string userName, string password)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = @"UPDATE Customers
                               SET Password = @password
                               WHERE Email = @userName";

            int result = await connection.ExecuteAsync(sql,
                new { userName, password });

            return result > 0;
        }

    }

    public async Task<List<string>> GetRolesAsync(string userName)
    {
        // Customers may not have roles, return empty list
        return await Task.FromResult(new List<string>());
    }

    public async Task<bool> UpdateRolesAsync(string userName, List<string> roles)
    {
        // Customers may not have roles, do nothing
        return await Task.FromResult(true);
    }

        public async Task<int> RegisterAsync(AccountCustomer data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Customers
                              (CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                              VALUES
                              (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }
    }

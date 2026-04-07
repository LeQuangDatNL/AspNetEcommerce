using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Security;

namespace SV22T1020065.DataLayers.Repository;

public class EmployeeAccountRepository : IUserAccountRepository
{
    private readonly string _connectionString;

    public EmployeeAccountRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT 
                                    EmployeeID   AS UserId,
                                    Email        AS UserName,
                                    FullName     AS DisplayName,
                                    Email        AS Email,
                                    Photo        AS Photo,
                                    RoleNames
                               FROM Employees
                               WHERE Email = @userName AND Password = @password";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql,
                new { userName, password });
        }
    }

    public async Task<bool> ChangePassword(string userName, string password)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            string sql = @"UPDATE Employees
                               SET Password = @password
                               WHERE Email = @userName";

            int result = await connection.ExecuteAsync(sql,
                new { userName, password });

            return result > 0;
        }
    }
}
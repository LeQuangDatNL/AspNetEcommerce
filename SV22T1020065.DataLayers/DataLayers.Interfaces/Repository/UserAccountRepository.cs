using Dapper;
using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.Models.Security;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace SV22T1020065.DataLayers.MySQL
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly IDbConnection _connection;

        public UserAccountRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Kiểm tra đăng nhập
        /// </summary>
        public async Task<UserAccount?> Authorize(string userName, string password)
        {
            var passwordHash = ComputeHash(password);

            var sql = @"SELECT UserId, UserName, DisplayName, Email, Photo, RoleNames
                        FROM Users
                        WHERE UserName = @UserName AND PasswordHash = @PasswordHash";

            return await _connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql,
                new { UserName = userName, PasswordHash = passwordHash }
            );
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<bool> ChangePassword(string userName, string password)
        {
            var passwordHash = ComputeHash(password);

            var sql = @"UPDATE Users 
                        SET PasswordHash = @PasswordHash
                        WHERE UserName = @UserName";

            return await _connection.ExecuteAsync(sql, new { PasswordHash = passwordHash, UserName = userName }) > 0;
        }

        /// <summary>
        /// Hàm hash mật khẩu bằng SHA256
        /// </summary>
        private string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
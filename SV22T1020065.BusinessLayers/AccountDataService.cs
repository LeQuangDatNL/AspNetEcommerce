using SV22T1020065.DataLayers.Interfaces;
using SV22T1020065.DataLayers.Repository;
using SV22T1020065.DataLayers;
using SV22T1020065.Models.Security;

namespace SV22T1020065.BusinessLayers
{
    public static class AccountDataService
    {
        private static readonly IUserAccountRepository employeeDB;
        private static readonly IUserAccountRepository customerDB;
        private static readonly CustomerAccountRepository customerAccountDB;

        static AccountDataService()
        {
            employeeDB = new EmployeeAccountRepository(Configuration.ConnectionString);
            customerDB = new CustomerAccountRepository(Configuration.ConnectionString);
            customerAccountDB = new CustomerAccountRepository(Configuration.ConnectionString);
        }

        #region LOGIN

        /// <summary>
        /// Đăng nhập nhân viên
        /// </summary>
        public static async Task<UserAccount?> LoginEmployeeAsync(string userName, string password)
        {
            return await employeeDB.AuthorizeAsync(userName, password);
        }

        /// <summary>
        /// Đăng nhập khách hàng
        /// </summary>
        public static async Task<UserAccount?> LoginCustomerAsync(string userName, string password)
        {
            return await customerDB.AuthorizeAsync(userName, password);
        }

        #endregion

        #region CHANGE PASSWORD

        /// <summary>
        /// Đổi mật khẩu nhân viên
        /// </summary>
        public static async Task<bool> ChangeEmployeePasswordAsync(string userName, string password)
        {
            return await employeeDB.ChangePassword(userName, password);
        }

        /// <summary>
        /// Đổi mật khẩu khách hàng
        /// </summary>
        public static async Task<bool> ChangeCustomerPasswordAsync(string userName, string password)
        {
            return await customerDB.ChangePassword(userName, password);
        }

        /// <summary>
        /// Đăng ký khách hàng mới
        /// </summary>
        public static async Task<int> RegisterCustomerAsync(SV22T1020065.Models.Models.Account.AccountCustomer data)
        {
            return await customerAccountDB.RegisterAsync(data);
        }

        #endregion
    }
}
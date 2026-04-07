using SV22T1020065.Models.Partner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020065.Models.Models.Account
{
    /// <summary>
    /// Dùng để đăng ký
    /// </summary>
    public class AccountCustomer : Customer
    {
        /// <summary>
        /// Mật khẩu của khách hàng
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}

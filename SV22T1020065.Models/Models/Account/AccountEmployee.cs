using SV22T1020065.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020065.Models.Models.Account
{
    class AccountEmployee:Employee
    {
        /// <summary>
        /// Mật khẩu (đã được mã hóa bằng thuật toán MD5)
        /// </summary>
        public string Password { get; set; } = "d3c84bdd44cb9ca888e0f2b19a5443ba";
        /// <summary>
        /// Quyền
        /// </summary>
        public string RoleNames { get; set; } = "employee";
    }
}

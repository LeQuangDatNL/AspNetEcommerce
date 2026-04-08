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
        public string Password { get; set; } = "";
        /// <summary>
        /// Quyền
        /// </summary>
        public List<string> RolesName { get; set; } = new List<string>();
    }
}

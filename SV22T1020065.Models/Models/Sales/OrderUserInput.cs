using SV22T1020065.Models.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020065.Models.Models.Sales
{
    public class OrderUserInput : OrderSearchInput
    {
        /// <summary>
        /// ID người dùng muốn lọc (nullable: null = tất cả)
        /// </summary>
        public int? CustomerID { get; set; } = null;
    }
}

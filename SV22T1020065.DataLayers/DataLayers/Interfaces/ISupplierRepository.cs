using SV22T1020065.Models.Partner;
using System.Threading.Tasks;
using SV22T1020065.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020065.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Supplier
    /// </summary>
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không?
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của nhà cung cấp mới.
        /// Nếu id <> 0: Kiểm tra email đối với nhà cung cấp đã tồn tại
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.AppCodes
{
    /// <summary>
    /// Biểu diễn kết quả trả về của API
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        ///  Mã lỗi (0: thành công, khác 0: thất bại)
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Thông báo lỗi (nếu có)
        /// </summary>
        public string Message { get; set; } = "";
        /// <summary>
        /// Thông báo lõi (nếu có)
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public ApiResult(int code, string message)
        {
            Code = code;
            Message = message;
        }
        
    }
}

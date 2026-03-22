using System;
namespace SV22T1020065.BusinessLayers
{
    /// <summary>
    /// Lưu trữ các thông tin cấu hình sử dụng trong business layer
    /// </summary>
    public static class Configuration
    {
        private static string _connectionString = "";

        /// <summary>
        /// Hàm có chức nawg khởi tạo cấu hình cho Buisiness Layeer
        /// Hàm này phải gọi trước khi chạy ứng dụng
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        public static string ConnectionString => _connectionString;
    }

}
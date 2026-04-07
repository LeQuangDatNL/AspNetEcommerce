namespace SV22T1020065.Models.Catalog
{
    /// <summary>
    /// ViewModel cho trang chi tiết sản phẩm
    /// </summary>
    public class ProductDetailViewModel
    {
        /// <summary>
        /// Thông tin sản phẩm
        /// </summary>
        public Product Product { get; set; } = new Product();

        /// <summary>
        /// Danh sách ảnh sản phẩm
        /// </summary>
        public List<ProductPhoto> Photos { get; set; } = new List<ProductPhoto>();

        /// <summary>
        /// Danh sách thuộc tính sản phẩm
        /// </summary>
        public List<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    }
}
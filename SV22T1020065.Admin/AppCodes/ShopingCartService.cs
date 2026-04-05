using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using SV22T1020065.Models.Sales;
using System.Threading.Tasks;

namespace SV22T1020065.Admin.AppCodes
{
    /// <summary>
    /// Cung cấp các chức năng xử lý trên giỏ hàng, bao gồm thêm, sửa, xóa sản phẩm trong giỏ hàng, tính tổng tiền, v.v.
    /// Giỏ hàng lưu bằng sesion
    /// </summary>
    public static class ShoppingCartService
    {
        /// <summary
        /// Tên biến để lưu giỏ hàng trong session
        /// </summary>
        private static readonly string SESSION_KEY = "ShoppingCart";
        // Lấy giỏ hàng từ session
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(SESSION_KEY);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(SESSION_KEY, cart);
            }
            return cart;
        }
        // Lấy thông tin sản phẩm trong giỏ hàng
        public static OrderDetailViewInfo? GetCartItem(int productId)
        {
            var cart = GetShoppingCart();
            return cart.FirstOrDefault(c => c.ProductID == productId);
        }
        // Thêm sản phẩm vào giỏ hàng
        public static void AddToCart(OrderDetailViewInfo item)
        {
            var cart = GetShoppingCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductID == item.ProductID);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
                existingItem.SalePrice = item.SalePrice;
            }
            else
            {
                cart.Add(item);
            }
            ApplicationContext.SetSessionData(SESSION_KEY, cart);
        }
        // Cập nhật số lượng sản phẩm trong giỏ hàng
        public static void UpdateCartItem(int productId, int quantity, int salePrice)
        {
            var cart = GetShoppingCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductID == productId);
            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                existingItem.SalePrice = salePrice;
                ApplicationContext.SetSessionData(SESSION_KEY, cart);
            }
        }
        // Xóa sản phẩm khỏi giỏ hàng
        public static void RemoveFromCart(int productId)
        {
            var cart = GetShoppingCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductID == productId);
            if (existingItem != null)
            {
                cart.Remove(existingItem);
                ApplicationContext.SetSessionData(SESSION_KEY, cart);
            }
        }
        // Xóa toàn bô giỏ hàng
        public static void ClearCart()
        {
            ApplicationContext.SetSessionData(SESSION_KEY, new List<OrderDetailViewInfo>());
        }
    }

}

using System.Collections.Generic;

namespace SV22T1020065.Models.Sales
{
    public class OrderDetailsViewModel
    {
        public OrderViewInfo Order { get; set; } = new OrderViewInfo();
        public List<OrderDetailViewInfo> Details { get; set; } = new List<OrderDetailViewInfo>();
    }
}

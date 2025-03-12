using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            Refunds = new HashSet<Refund>();
        }

        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ShopId { get; set; }
        public int EmployeeId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalAmount { get; set; }
        public int PaymentMethod { get; set; }

        public virtual Employee Employee { get; set; } = null!;
        public virtual Warehouse Shop { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Refund> Refunds { get; set; }
    }
}

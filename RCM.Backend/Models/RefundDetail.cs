using System;
using System.Collections.Generic;

namespace RCM.Backend.Models
{
    public partial class RefundDetail
    {
        public int Id { get; set; }
        public int RefundId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public virtual Refund Refund { get; set; } = null!;
    }
}

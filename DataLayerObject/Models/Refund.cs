using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class Refund
    {
        public Refund()
        {
            RefundDetails = new HashSet<RefundDetail>();
        }

        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime RefundDate { get; set; }
        public decimal RefundAmount { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual ICollection<RefundDetail> RefundDetails { get; set; }
    }
}

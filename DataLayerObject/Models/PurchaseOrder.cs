using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class PurchaseOrder
    {
        public PurchaseOrder()
        {
            PurchaseOrderItems = new HashSet<PurchaseOrderItem>();
        }

        public int Id { get; set; }
        public int? SupplierId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedArrival { get; set; }
        public string? Status { get; set; }

        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}

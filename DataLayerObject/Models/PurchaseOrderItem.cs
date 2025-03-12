using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ProductId { get; set; }
        public int BatchId { get; set; }
        public int QuantityOrdered { get; set; }
        public int? QuantityReceived { get; set; }
        public decimal Price { get; set; }

        public virtual Batch Batch { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    }
}

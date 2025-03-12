using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class Batch
    {
        public Batch()
        {
            BatchDetails = new HashSet<BatchDetail>();
            PurchaseOrderItems = new HashSet<PurchaseOrderItem>();
        }

        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public DateTime? ReceivedDate { get; set; }

        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<BatchDetail> BatchDetails { get; set; }
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}

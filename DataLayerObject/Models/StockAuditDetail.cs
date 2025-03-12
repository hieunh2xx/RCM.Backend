using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class StockAuditDetail
    {
        public int Id { get; set; }
        public int AuditId { get; set; }
        public int ProductId { get; set; }
        public int RecordedQuantity { get; set; }

        public virtual StockAuditRecord Audit { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}

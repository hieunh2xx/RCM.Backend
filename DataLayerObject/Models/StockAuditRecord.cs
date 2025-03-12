using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class StockAuditRecord
    {
        public StockAuditRecord()
        {
            StockAuditDetails = new HashSet<StockAuditDetail>();
        }

        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public DateTime AuditDate { get; set; }

        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<StockAuditDetail> StockAuditDetails { get; set; }
    }
}

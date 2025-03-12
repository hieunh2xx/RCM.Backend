using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class StockAdjustment
    {
        public StockAdjustment()
        {
            StockAdjustmentDetails = new HashSet<StockAdjustmentDetail>();
        }

        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public DateTime AdjustmentDate { get; set; }

        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<StockAdjustmentDetail> StockAdjustmentDetails { get; set; }
    }
}

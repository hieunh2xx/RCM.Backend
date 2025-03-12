using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class StockAdjustmentDetail
    {
        public int Id { get; set; }
        public int AdjustmentId { get; set; }
        public int ProductId { get; set; }
        public int AdjustedQuantity { get; set; }

        public virtual StockAdjustment Adjustment { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}

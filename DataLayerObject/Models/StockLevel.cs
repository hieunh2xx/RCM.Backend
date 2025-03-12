using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class StockLevel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class Product
    {
        public Product()
        {
            BatchDetails = new HashSet<BatchDetail>();
            OrderDetails = new HashSet<OrderDetail>();
            ProductPrices = new HashSet<ProductPrice>();
            PurchaseOrderItems = new HashSet<PurchaseOrderItem>();
            StockAdjustmentDetails = new HashSet<StockAdjustmentDetail>();
            StockAuditDetails = new HashSet<StockAuditDetail>();
            StockLevels = new HashSet<StockLevel>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Barcode { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public int QuantityPerUnit { get; set; }
        public string BaseUnit { get; set; } = null!;
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public bool? IsEnabled { get; set; }

        public virtual ICollection<BatchDetail> BatchDetails { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ProductPrice> ProductPrices { get; set; }
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public virtual ICollection<StockAdjustmentDetail> StockAdjustmentDetails { get; set; }
        public virtual ICollection<StockAuditDetail> StockAuditDetails { get; set; }
        public virtual ICollection<StockLevel> StockLevels { get; set; }
    }
}

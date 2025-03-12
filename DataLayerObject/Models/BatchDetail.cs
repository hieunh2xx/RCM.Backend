using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class BatchDetail
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }

        public virtual Batch Batch { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}

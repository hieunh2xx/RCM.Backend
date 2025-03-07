using System;
using System.Collections.Generic;

namespace RCM.Backend.Models
{
    public partial class ProductPrice
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public DateTime? EffectiveDate { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class PenaltyPayment
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Reason { get; set; } = null!;
        public int PaymentMethod { get; set; }
        public string? Note { get; set; }
        public bool? IsDeleted { get; set; }

        public virtual Employee Employee { get; set; } = null!;
    }
}

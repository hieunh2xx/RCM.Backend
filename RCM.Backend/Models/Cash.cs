using System;
using System.Collections.Generic;

namespace RCM.Backend.Models
{
    public partial class Cash
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }
        public int PaymentMethod { get; set; }
        public bool? Type { get; set; }
        public int Amount { get; set; }
        public DateTime? Date { get; set; }
        public string? Note { get; set; }

        public virtual Warehouse Branch { get; set; } = null!;
        public virtual Employee Employee { get; set; } = null!;
    }
}

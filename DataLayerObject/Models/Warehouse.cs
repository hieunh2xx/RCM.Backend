using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class Warehouse
    {
        public Warehouse()
        {
            Batches = new HashSet<Batch>();
            Cashes = new HashSet<Cash>();
            Employees = new HashSet<Employee>();
            Orders = new HashSet<Order>();
            StockAdjustments = new HashSet<StockAdjustment>();
            StockAuditRecords = new HashSet<StockAuditRecord>();
            StockLevels = new HashSet<StockLevel>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Capacity { get; set; }

        public virtual ICollection<Batch> Batches { get; set; }
        public virtual ICollection<Cash> Cashes { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<StockAdjustment> StockAdjustments { get; set; }
        public virtual ICollection<StockAuditRecord> StockAuditRecords { get; set; }
        public virtual ICollection<StockLevel> StockLevels { get; set; }
    }
}

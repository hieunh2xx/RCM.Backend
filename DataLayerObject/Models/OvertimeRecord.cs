using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class OvertimeRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalHours { get; set; }
        public string? Reason { get; set; }
        public bool? IsApproved { get; set; }

        public virtual Employee Employee { get; set; } = null!;
    }
}

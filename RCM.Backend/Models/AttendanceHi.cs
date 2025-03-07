using System;
using System.Collections.Generic;

namespace RCM.Backend.Models
{
    public partial class AttendanceHi
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Shift { get; set; } = null!;
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public int? OnTime { get; set; }

        public virtual Employee Employee { get; set; } = null!;
    }
}

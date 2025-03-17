using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class AttendanceCheckIn
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Shift { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public int? OnTime { get; set; }

        public virtual Employee Employee { get; set; } = null!;
    }
}

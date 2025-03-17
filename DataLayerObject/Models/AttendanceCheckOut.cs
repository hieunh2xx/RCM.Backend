using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class AttendanceCheckOut
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Shift { get; set; } = null!;
        public DateTime CheckOutTime { get; set; }

        public virtual Employee Employee { get; set; } = null!;
    }
}

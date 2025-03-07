using System;
using System.Collections.Generic;

namespace RCM.Backend.Models
{
    public partial class Salary
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int? FixedSalary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BonusSalary { get; set; }
        public int? Penalty { get; set; }
        public int? FinalSalary { get; set; }

        public virtual Employee Employee { get; set; } = null!;
    }
}

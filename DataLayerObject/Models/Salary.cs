using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class Salary
    {
        public Salary()
        {
            SalaryPaymentHistories = new HashSet<SalaryPaymentHistory>();
        }

        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int? FixedSalary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BonusSalary { get; set; }
        public int? FinalSalary { get; set; }
        public string? Status { get; set; }

        public virtual Employee Employee { get; set; } = null!;
        public virtual ICollection<SalaryPaymentHistory> SalaryPaymentHistories { get; set; }
    }
}

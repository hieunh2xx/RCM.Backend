using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class Employee
    {
        public Employee()
        {
            Accounts = new HashSet<Account>();
            AttendanceCheckIns = new HashSet<AttendanceCheckIn>();
            AttendanceCheckOuts = new HashSet<AttendanceCheckOut>();
            Cashes = new HashSet<Cash>();
            Orders = new HashSet<Order>();
            OvertimeRecords = new HashSet<OvertimeRecord>();
            PenaltyPayments = new HashSet<PenaltyPayment>();
            Salaries = new HashSet<Salary>();
            SalaryPaymentHistories = new HashSet<SalaryPaymentHistory>();
        }

        public int Id { get; set; }
        public string? Image { get; set; }
        public string FullName { get; set; } = null!;
        public string? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string IdentityNumber { get; set; } = null!;
        public string? Hometown { get; set; }
        public string? CurrentAddress { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public int? WorkShiftId { get; set; }
        public int? FixedSalary { get; set; }
        public bool? ActiveStatus { get; set; }
        public DateTime StartDate { get; set; }
        public int? BranchId { get; set; }
        public bool? SeniorityStatus { get; set; }

        public virtual Warehouse? Branch { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<AttendanceCheckIn> AttendanceCheckIns { get; set; }
        public virtual ICollection<AttendanceCheckOut> AttendanceCheckOuts { get; set; }
        public virtual ICollection<Cash> Cashes { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<OvertimeRecord> OvertimeRecords { get; set; }
        public virtual ICollection<PenaltyPayment> PenaltyPayments { get; set; }
        public virtual ICollection<Salary> Salaries { get; set; }
        public virtual ICollection<SalaryPaymentHistory> SalaryPaymentHistories { get; set; }
    }
}

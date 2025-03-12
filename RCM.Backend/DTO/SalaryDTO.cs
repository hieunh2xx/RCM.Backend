namespace RCM.Backend.DTO
{
    public class SalaryDTO
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int? FixedSalary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BonusSalary { get; set; }
        public int? FinalSalary { get; set; }
        public string Status { get; set; } = "Pending";
    }
  

}

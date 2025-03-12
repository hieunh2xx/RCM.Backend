namespace RCM.Backend.DTO
{
    public class SalaryPaymentHistoryDTO
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int SalaryId { get; set; }

        public DateTime PaymentDate { get; set; }

        public int PaidAmount { get; set; }

        public int PaymentMethod { get; set; }

        public string? Note { get; set; }

        public bool? IsDeleted { get; set; }
    }
}

using DataLayerObject.Models;

public class EmployeeDTO
{
    public int Id { get; set; }
    public string? Image { get; set; }
    public string FullName { get; set; } = null!;
    public string Username { get; set; }
    public byte Role { get; set; }
    public string? Gender { get; set; }
    public DateTime BirthDate { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public int? WorkShiftId { get; set; }
    public bool? ActiveStatus { get; set; }
    public DateTime StartDate { get; set; }
    public int? BranchId { get; set; }
    public bool IsStaff { get; set; }
    public string PasswordHash { get; set; } = null!;
    public decimal PenaltyAmount { get; set; }
    public string Note { get; set; } = null!;
    public string TotalPenaltyAmount { get; set; } = null!;

    // Các thuộc tính đang thiếu
    public string IdentityNumber { get; set; } = null!;
    public string? Hometown { get; set; }
    public string? CurrentAddress { get; set; }
    public int? FixedSalary { get; set; }
    public class StaffExportDTO
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public int? WorkShiftId { get; set; }
        public bool? ActiveStatus { get; set; }
        public DateTime StartDate { get; set; }
        public int? BranchId { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }
    public static EmployeeDTO MapToDTO(Employee employee)
    {
        return new EmployeeDTO
        {
            Id = employee.Id,
            Image = employee.Image,
            FullName = employee.FullName,
            Gender = employee.Gender,
            BirthDate = employee.BirthDate,
            PhoneNumber = employee.PhoneNumber,
            WorkShiftId = employee.WorkShiftId,
            ActiveStatus = employee.ActiveStatus,
            StartDate = employee.StartDate,
            BranchId = employee.BranchId,
            IsStaff = DetermineIfStaff(employee),

            // Gán thêm các thuộc tính mới
            IdentityNumber = employee.IdentityNumber,
            Hometown = employee.Hometown,
            CurrentAddress = employee.CurrentAddress,
            FixedSalary = employee.FixedSalary
        };
    }

    // Hàm kiểm tra nhân viên có phải Staff hay không
    private static bool DetermineIfStaff(Employee employee)
    {
        return employee.WorkShiftId.HasValue;
    }
}

namespace RCM.Backend.DTO
{
    public class AccountDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int EmployeeId { get; set; }
        public Byte Role { get; set; }
    }
    public enum UserRole : byte
    {
        Guest = 0,
        User = 1,
        Manager = 2,
        Admin = 3
    }

}

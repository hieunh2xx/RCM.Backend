using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RCM.Backend.DTO;
using RCM.Backend.Models;

namespace RCM.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly RetailChainContext _context;
        public StaffController(RetailChainContext context)
        {
            _context = context;
        }
        [HttpGet("getStaff")]
        public IActionResult GetAllStaff()
        {
            var staffList = _context.Employees
                .Join(_context.Accounts,
                      e => e.Id,
                      a => a.EmployeeId,
                      (e, a) => new { Employee = e, Account = a }) // Kết hợp Employees với Accounts
                .Where(ea => ea.Employee.WorkShiftId.HasValue && ea.Account.Role == 2) // Lọc Staff có Role = 2
                .Select(ea => new EmployeeDTO
                {
                    Id = ea.Employee.Id,
                    Image = ea.Employee.Image,
                    FullName = ea.Employee.FullName,
                    Gender = ea.Employee.Gender,
                    BirthDate = ea.Employee.BirthDate,
                    PhoneNumber = ea.Employee.PhoneNumber,
                    WorkShiftId = ea.Employee.WorkShiftId,
                    ActiveStatus = ea.Employee.ActiveStatus,
                    StartDate = ea.Employee.StartDate,
                    BranchId = ea.Employee.BranchId,
                    IsStaff = true,
                    Username = ea.Account.Username,
                    Role = (byte)(ea.Account.Role ?? 0) // Fix lỗi ép kiểu byte?
                })
                .ToList();

            return Ok(staffList);
        }


        [HttpGet("{id}")]
        public IActionResult GetStaffById(int id)
        {
            var employeeData = _context.Employees
                .Join(_context.Accounts,
                      e => e.Id,
                      a => a.EmployeeId,
                      (e, a) => new { Employee = e, Account = a }) // Kết hợp Employees với Accounts
                .Where(ea => ea.Employee.Id == id && ea.Employee.WorkShiftId.HasValue && ea.Account.Role == 2)
                .Select(ea => new EmployeeDTO
                {
                    Id = ea.Employee.Id,
                    Image = ea.Employee.Image,
                    FullName = ea.Employee.FullName,
                    Gender = ea.Employee.Gender,
                    BirthDate = ea.Employee.BirthDate,
                    PhoneNumber = ea.Employee.PhoneNumber,
                    WorkShiftId = ea.Employee.WorkShiftId,
                    ActiveStatus = ea.Employee.ActiveStatus,
                    StartDate = ea.Employee.StartDate,
                    BranchId = ea.Employee.BranchId,
                    IsStaff = true,
                    Username = ea.Account.Username,
                    Role = (byte)(ea.Account.Role ?? 0) // Fix lỗi ép kiểu byte?
                })
                .FirstOrDefault();

            if (employeeData == null)
            {
                return NotFound(new { message = "Staff not found!" });
            }

            return Ok(employeeData);
        }


        [HttpPost("add-account")]
        public IActionResult AddAccountForStaff([FromBody] AccountDTO request)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == request.EmployeeId && e.WorkShiftId.HasValue);
            if (employee == null)
            {
                return NotFound(new { message = "Staff not found!" });
            }

            // Kiểm tra xem nhân viên đã có tài khoản chưa
            bool accountExists = _context.Accounts.Any(a => a.EmployeeId == request.EmployeeId);
            if (accountExists)
            {
                return BadRequest(new { message = "This staff already has an account!" });
            }

            // Mã hoá mật khẩu (nếu có hash)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo tài khoản mới
            var newAccount = new Account
            {
                EmployeeId = request.EmployeeId,
                Username = request.Username,
                PasswordHash = hashedPassword,
                Role = request.Role
            };

            _context.Accounts.Add(newAccount);
            _context.SaveChanges();

            return Ok(new { message = "Account created successfully!" });
        }

        [HttpPost("add-employee")]
        public IActionResult AddEmployee([FromBody] EmployeeDTO request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid request data!" });
            }

            bool exists = _context.Employees.Any(e => e.IdentityNumber == request.IdentityNumber || e.PhoneNumber == request.PhoneNumber);
            if (exists)
            {
                return BadRequest(new { message = "Identity number or phone number already exists!" });
            }

            var newEmployee = new Employee
            {
                Image = request.Image,
                FullName = request.FullName,
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                IdentityNumber = request.IdentityNumber,
                Hometown = request.Hometown,
                CurrentAddress = request.CurrentAddress,
                PhoneNumber = request.PhoneNumber,
                WorkShiftId = request.WorkShiftId,
                FixedSalary = request.FixedSalary,
                ActiveStatus = request.ActiveStatus ?? true, 
                StartDate =  DateTime.Now,
                BranchId = request.BranchId
            };

            _context.Employees.Add(newEmployee);
            _context.SaveChanges();

            return Ok(new { message = "Employee added successfully!", EmployeeId = newEmployee.Id });
        }

    }
}

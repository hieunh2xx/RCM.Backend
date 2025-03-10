using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RCM.Backend.DTO;
using RCM.Backend.Models;
using System.Globalization;
using static EmployeeDTO;

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
        public IActionResult GetAllStaff([FromQuery] string? name)
        {
            var query = _context.Employees
                .Join(_context.Accounts,
                      e => e.Id,
                      a => a.EmployeeId,
                      (e, a) => new { Employee = e, Account = a }) 
                .Where(ea => ea.Employee.WorkShiftId.HasValue && ea.Account.Role == 2); 

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(ea => ea.Employee.FullName.Contains(name));
            }

            var staffList = query
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
                    Role = (byte)(ea.Account.Role ?? 0)
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

        [HttpPut("update-employee/{id}")]
        public IActionResult UpdateEmployee(int id, [FromBody] EmployeeDTO request)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found!" });
            }

            bool exists = _context.Employees.Any(e =>
                (e.IdentityNumber == request.IdentityNumber || e.PhoneNumber == request.PhoneNumber)
                && e.Id != id);
            if (exists)
            {
                return BadRequest(new { message = "Identity number or phone number already exists!" });
            }

            // Cập nhật thông tin nhân viên
            employee.Image = request.Image ?? employee.Image;
            employee.FullName = request.FullName ?? employee.FullName;
            employee.Gender = request.Gender ?? employee.Gender;
            employee.BirthDate = request.BirthDate;
            employee.IdentityNumber = request.IdentityNumber ?? employee.IdentityNumber;
            employee.Hometown = request.Hometown ?? employee.Hometown;
            employee.CurrentAddress = request.CurrentAddress ?? employee.CurrentAddress;
            employee.PhoneNumber = request.PhoneNumber ?? employee.PhoneNumber;
            employee.WorkShiftId = request.WorkShiftId ?? employee.WorkShiftId;
            employee.FixedSalary = request.FixedSalary ?? employee.FixedSalary;
            employee.ActiveStatus = request.ActiveStatus ?? employee.ActiveStatus;
            employee.BranchId = request.BranchId ?? employee.BranchId;

            _context.SaveChanges();

            return Ok(new { message = "Employee updated successfully!" });
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
        [HttpPost("import")]
        public IActionResult ImportEmployees(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file để upload!" });
            }

            var employees = new List<Employee>();

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension == ".xlsx" || extension == ".xls")
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // Bỏ qua tiêu đề
                        {
                            string roleValue = worksheet.Cells[row, 11].Value?.ToString();
                            byte role = (roleValue == "Staff") ? (byte)2 : (byte)0; // Chuyển "Staff" thành 2

                            var employee = new Employee
                            {
                                FullName = worksheet.Cells[row, 1].Value?.ToString(),
                                Gender = worksheet.Cells[row, 2].Value?.ToString(),
                                BirthDate = DateTime.Parse(worksheet.Cells[row, 3].Value?.ToString()),
                                IdentityNumber = worksheet.Cells[row, 4].Value?.ToString(),
                                Hometown = worksheet.Cells[row, 5].Value?.ToString(),
                                CurrentAddress = worksheet.Cells[row, 6].Value?.ToString(),
                                PhoneNumber = worksheet.Cells[row, 7].Value?.ToString(),
                                WorkShiftId = int.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var shiftId) ? shiftId : (int?)null,
                                FixedSalary = int.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out var salary) ? salary : 0,
                                ActiveStatus = true,
                                StartDate = DateTime.Now,
                                BranchId = int.Parse(worksheet.Cells[row, 10].Value?.ToString())
                            };

                            var newAccount = new Account
                            {
                                EmployeeId = employee.Id,
                                Username = worksheet.Cells[row, 12].Value?.ToString(),
                                PasswordHash = BCrypt.Net.BCrypt.HashPassword("defaultpassword"), // Hash password mặc định
                                Role = role
                            };

                            employees.Add(employee);
                            _context.Accounts.Add(newAccount);
                        }
                    }
                }
            }

            else if (extension == ".csv")
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<EmployeeDTO>().ToList();
                    employees = records.Select(r => new Employee
                    {
                        FullName = r.FullName,
                        Gender = r.Gender,
                        BirthDate = r.BirthDate,
                        IdentityNumber = r.IdentityNumber,
                        Hometown = r.Hometown,
                        CurrentAddress = r.CurrentAddress,
                        PhoneNumber = r.PhoneNumber,
                        WorkShiftId = r.WorkShiftId,
                        FixedSalary = r.FixedSalary,
                        ActiveStatus = true,
                        StartDate = DateTime.Now,
                        BranchId = r.BranchId
                    }).ToList();
                }
            }
            else
            {
                return BadRequest(new { message = "Chỉ hỗ trợ file Excel (.xlsx, .xls) hoặc CSV!" });
            }

            if (employees.Count == 0)
            {
                return BadRequest(new { message = "Không có dữ liệu hợp lệ trong file!" });
            }

            _context.Employees.AddRange(employees);
            _context.SaveChanges();

            return Ok(new { message = "Import thành công!", totalEmployees = employees.Count });
        }
        [HttpGet("export")]
        public IActionResult ExportStaff([FromQuery] string? format = "xlsx")
        {
            var staffList = _context.Employees
                .Join(_context.Accounts,
                      e => e.Id,
                      a => a.EmployeeId,
                      (e, a) => new { Employee = e, Account = a })
                .Where(ea => ea.Employee.WorkShiftId.HasValue && ea.Account.Role == 2) 
                .Select(ea => new StaffExportDTO
                {
                    Id = ea.Employee.Id,
                    FullName = ea.Employee.FullName,
                    Gender = ea.Employee.Gender,
                    BirthDate = ea.Employee.BirthDate,
                    PhoneNumber = ea.Employee.PhoneNumber,
                    WorkShiftId = ea.Employee.WorkShiftId,
                    ActiveStatus = ea.Employee.ActiveStatus,
                    StartDate = ea.Employee.StartDate,
                    BranchId = ea.Employee.BranchId,
                    Username = ea.Account.Username,
                    Role = ea.Account.Role == 2 ? "Staff" : "Unknown"
                })
                .ToList();

            if (staffList.Count == 0)
            {
                return NotFound(new { message = "Không có dữ liệu để export!" });
            }

            if (format.ToLower() == "csv")
            {
                return ExportToCsv(staffList);
            }
            else
            {
                return ExportToExcel(staffList);
            }
        }

        /// <summary>
        /// Xuất file Excel từ danh sách nhân viên.
        /// </summary>
        private IActionResult ExportToExcel(List<StaffExportDTO> staffList)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Staff Data");

                // Header
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Full Name";
                worksheet.Cells[1, 3].Value = "Gender";
                worksheet.Cells[1, 4].Value = "Birth Date";
                worksheet.Cells[1, 5].Value = "Phone Number";
                worksheet.Cells[1, 6].Value = "Work Shift ID";
                worksheet.Cells[1, 7].Value = "Active Status";
                worksheet.Cells[1, 8].Value = "Start Date";
                worksheet.Cells[1, 9].Value = "Branch ID";
                worksheet.Cells[1, 10].Value = "Username";
                worksheet.Cells[1, 11].Value = "Role";

                // Data
                for (int i = 0; i < staffList.Count; i++)
                {
                    var staff = staffList[i];
                    worksheet.Cells[i + 2, 1].Value = staff.Id;
                    worksheet.Cells[i + 2, 2].Value = staff.FullName;
                    worksheet.Cells[i + 2, 3].Value = staff.Gender;
                    worksheet.Cells[i + 2, 4].Value = staff.BirthDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 5].Value = staff.PhoneNumber;
                    worksheet.Cells[i + 2, 6].Value = staff.WorkShiftId;
                    worksheet.Cells[i + 2, 7].Value = staff.ActiveStatus;
                    worksheet.Cells[i + 2, 8].Value = staff.StartDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 9].Value = staff.BranchId;
                    worksheet.Cells[i + 2, 10].Value = staff.Username;
                    worksheet.Cells[i + 2, 11].Value = staff.Role;
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"StaffData_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        /// <summary>
        /// Xuất file CSV từ danh sách nhân viên.
        /// </summary>
        private IActionResult ExportToCsv(List<StaffExportDTO> staffList)
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, System.Text.Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(staffList);
            }

            stream.Position = 0;
            string fileName = $"StaffData_{DateTime.Now:yyyyMMddHHmmss}.csv";
            return File(stream, "text/csv", fileName);
        }

    }

}


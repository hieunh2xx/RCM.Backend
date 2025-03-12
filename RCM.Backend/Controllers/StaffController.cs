using ClosedXML.Excel;
using CsvHelper;
using DataLayerObject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RCM.Backend.DTO;
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
            // Bước 1: Lấy thông tin nhân viên, tài khoản và lương
            var employeeData = _context.Employees
                .Join(_context.Accounts,
                      e => e.Id,
                      a => a.EmployeeId,
                      (e, a) => new { Employee = e, Account = a })
                .Join(_context.Salaries,
                      ea => ea.Employee.Id,
                      s => s.EmployeeId,
                      (ea, s) => new { ea.Employee, ea.Account, Salary = s })
                .Where(eas => eas.Employee.Id == id)
                .Select(eas => new EmployeeDTO
                {
                    Id = eas.Employee.Id,
                    Image = eas.Employee.Image,
                    FullName = eas.Employee.FullName,
                    Gender = eas.Employee.Gender,
                    BirthDate = eas.Employee.BirthDate,
                    PhoneNumber = eas.Employee.PhoneNumber,
                    WorkShiftId = eas.Employee.WorkShiftId,
                    ActiveStatus = eas.Employee.ActiveStatus,
                    StartDate = eas.Employee.StartDate,
                    BranchId = eas.Employee.BranchId,
                    IsStaff = true,
                    Username = eas.Account.Username,
                    Role = (byte)(eas.Account.Role ?? 0),
                    CurrentAddress = eas.Employee.CurrentAddress,
                    FixedSalary = eas.Salary.FixedSalary,
                    IdentityNumber = eas.Employee.IdentityNumber
                })
                .FirstOrDefault();

            if (employeeData == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên!" });
            }

            // Bước 2: Lấy danh sách phạt và tính tổng số tiền phạt
            var penalties = _context.PenaltyPayments
                .Where(p => p.EmployeeId == id)
                .Select(p => new { p.Amount, p.Note })
                .ToList(); // Chuyển sang xử lý trên bộ nhớ

            employeeData.PenaltyAmount = penalties.Sum(p => p.Amount);
            employeeData.Note = string.Join(", ", penalties.Select(p => p.Note));

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

        [HttpPost("add-employee")]
        public IActionResult AddStaff([FromBody] EmployeeDTO request)
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
                StartDate = DateTime.Now,
                BranchId = request.BranchId
            };

            _context.Employees.Add(newEmployee);
            _context.SaveChanges();

            var newAccount = new Account
            {
                EmployeeId = newEmployee.Id,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash),
                Role = request.Role
            };

            _context.Accounts.Add(newAccount);
            _context.SaveChanges();

            return Ok(new { message = "Staff added successfully!", EmployeeId = newEmployee.Id });
        }

        //[HttpPost("add-employee")]
        //public IActionResult AddEmployee([FromBody] EmployeeDTO request)
        //{
        //    if (request == null)
        //    {
        //        return BadRequest(new { message = "Invalid request data!" });
        //    }

        //    bool exists = _context.Employees.Any(e => e.IdentityNumber == request.IdentityNumber || e.PhoneNumber == request.PhoneNumber);
        //    if (exists)
        //    {
        //        return BadRequest(new { message = "Identity number or phone number already exists!" });
        //    }

        //    var newEmployee = new Employee
        //    {
        //        Image = request.Image,
        //        FullName = request.FullName,
        //        Gender = request.Gender,
        //        BirthDate = request.BirthDate,
        //        IdentityNumber = request.IdentityNumber,
        //        Hometown = request.Hometown,
        //        CurrentAddress = request.CurrentAddress,
        //        PhoneNumber = request.PhoneNumber,
        //        WorkShiftId = request.WorkShiftId,
        //        FixedSalary = request.FixedSalary,
        //        ActiveStatus = request.ActiveStatus ?? true, 
        //        StartDate =  DateTime.Now,
        //        BranchId = request.BranchId
        //    };

        //    _context.Employees.Add(newEmployee);
        //    _context.SaveChanges();

        //    return Ok(new { message = "Employee added successfully!", EmployeeId = newEmployee.Id });
        //}
        [HttpPost("import")]
        public IActionResult ImportStaff(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file để upload!" });
            }

            var employees = new List<Employee>();
            var accounts = new List<Account>();
            var existingPhoneNumbers = _context.Employees.Select(e => e.PhoneNumber).ToHashSet();
            var newPhoneNumbers = new HashSet<string>();

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
                        var duplicatePhoneNumbers = new List<string>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            string phoneNumber = worksheet.Cells[row, 4].Value?.ToString();

                            if (string.IsNullOrWhiteSpace(phoneNumber)) continue;

                            if (existingPhoneNumbers.Contains(phoneNumber) || newPhoneNumbers.Contains(phoneNumber))
                            {
                                duplicatePhoneNumbers.Add(phoneNumber);
                                continue;
                            }

                            newPhoneNumbers.Add(phoneNumber);

                            string roleValue = worksheet.Cells[row, 10].Value?.ToString();
                            byte role = (roleValue == "Staff") ? (byte)2 : (byte)0;

                            DateTime? birthDate = null;
                            if (DateTime.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out DateTime parsedDate))
                            {
                                birthDate = parsedDate;
                            }

                            int? workShiftId = null;
                            if (int.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out int shiftId))
                            {
                                workShiftId = shiftId;
                            }

                            int fixedSalary = 0;
                            if (int.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out int salary))
                            {
                                fixedSalary = salary;
                            }

                            int? branchId = null;
                            if (int.TryParse(worksheet.Cells[row, 10].Value?.ToString(), out int parsedBranchId))
                            {
                                branchId = parsedBranchId;
                            }

                            var employee = new Employee
                            {
                                FullName = worksheet.Cells[row, 1].Value?.ToString(),
                                Gender = worksheet.Cells[row, 2].Value?.ToString(),
                                BirthDate = birthDate ?? DateTime.MinValue,
                                IdentityNumber = worksheet.Cells[row, 4].Value?.ToString(),
                                Hometown = worksheet.Cells[row, 5].Value?.ToString(),
                                CurrentAddress = worksheet.Cells[row, 6].Value?.ToString(),
                                PhoneNumber = phoneNumber,
                                WorkShiftId = workShiftId,
                                FixedSalary = fixedSalary,
                                ActiveStatus = true,
                                StartDate = DateTime.Now,
                                BranchId = branchId
                            };

                            employees.Add(employee);
                        }

                        if (duplicatePhoneNumbers.Count > 0)
                        {
                            return BadRequest(new
                            {
                                message = "Một số nhân viên đã tồn tại hoặc có số điện thoại trùng lặp. Vui lòng kiểm tra lại file!",
                                duplicatePhoneNumbers
                            });
                        }
                    }
                }
            }
            else
            {
                return BadRequest(new { message = "Chỉ hỗ trợ file Excel (.xlsx, .xls) hoặc CSV!" });
            }

            _context.Employees.AddRange(employees);
            _context.SaveChanges();

            foreach (var emp in employees)
            {
                accounts.Add(new Account
                {
                    EmployeeId = emp.Id,
                    Username = emp.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789"),
                    Role = 2
                });
            }

            _context.Accounts.AddRange(accounts);
            _context.SaveChanges();

            return Ok(new { message = "Import thành công!", totalEmployees = employees.Count });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportStaff()
        {
            var staffList = await _context.Employees
                .Join(_context.Accounts,
                      e => e.Id,
                      a => a.EmployeeId,
                      (e, a) => new { Employee = e, Account = a })
                .Where(ea => ea.Employee.WorkShiftId.HasValue && ea.Account.Role == 2)
                .Select(ea => new
                {
                    ea.Employee.Id,
                    ea.Employee.FullName,
                    ea.Employee.Gender,
                    BirthDate = ea.Employee.BirthDate.ToString("yyyy-MM-dd"),
                    ea.Employee.PhoneNumber,
                    ea.Employee.WorkShiftId,
                    ea.Employee.ActiveStatus,
                    StartDate = ea.Employee.StartDate.ToString("yyyy-MM-dd"),
                    ea.Employee.BranchId,
                    ea.Account.Username,
                    Role = ea.Account.Role == 2 ? "Staff" : "Unknown"
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Staff Data");

            var headers = new string[]
            {
        "EmployeeID", "FullName", "Gender", "BirthDate", "PhoneNumber",
        "WorkShiftID", "ActiveStatus", "StartDate", "BranchID", "Username", "Role"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            int row = 2;
            foreach (var staff in staffList)
            {
                worksheet.Cell(row, 1).Value = staff.Id;
                worksheet.Cell(row, 2).Value = staff.FullName;
                worksheet.Cell(row, 3).Value = staff.Gender;
                worksheet.Cell(row, 4).Value = staff.BirthDate;
                worksheet.Cell(row, 5).Value = staff.PhoneNumber;
                worksheet.Cell(row, 6).Value = staff.WorkShiftId;
                worksheet.Cell(row, 7).Value = staff.ActiveStatus.HasValue && staff.ActiveStatus.Value ? "True" : "False";
                worksheet.Cell(row, 8).Value = staff.StartDate;
                worksheet.Cell(row, 9).Value = staff.BranchId;
                worksheet.Cell(row, 10).Value = staff.Username;
                worksheet.Cell(row, 11).Value = staff.Role;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StaffList.xlsx");
        }

        [HttpGet("download-template")]
        public IActionResult DownloadTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Staff Template");
                worksheet.Cell(1, 1).Value = "Full Name";
                worksheet.Cell(1, 2).Value = "Gender";
                worksheet.Cell(1, 3).Value = "Birth Date (yyyy-MM-dd)";
                worksheet.Cell(1, 4).Value = "Identity Number";
                worksheet.Cell(1, 5).Value = "Hometown";
                worksheet.Cell(1, 6).Value = "Current Address";
                worksheet.Cell(1, 7).Value = "Phone Number";
                worksheet.Cell(1, 8).Value = "Work Shift ID";
                worksheet.Cell(1, 9).Value = "Fixed Salary";
                worksheet.Cell(1, 10).Value = "Branch ID";
                worksheet.Cell(1, 11).Value = "Role (Staff=2, Admin=1)";

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Staff_Template.xlsx");
                }
            }
        }

    }

}


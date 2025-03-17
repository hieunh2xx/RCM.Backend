using ClosedXML.Excel;
using DataLayerObject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCM.Backend.DTO;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class PayrollController : ControllerBase
{
    private readonly RetailChainContext _context;

    public PayrollController(RetailChainContext context)
    {
        _context = context;
    }

    [HttpPost("getAllPayroll")]
    public async Task<IActionResult> CalculateAndSavePayrollForAllEmployees(
        [FromQuery] string? search,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        bool payrollExists = await _context.Salaries
            .AnyAsync(s => s.StartDate.HasValue &&
                           s.StartDate.Value.Month == month &&
                           s.StartDate.Value.Year == year);

        var employeesQuery = _context.Employees.AsNoTracking();
        if (!string.IsNullOrEmpty(search))
        {
            employeesQuery = employeesQuery.Where(e => e.FullName.Contains(search) || e.PhoneNumber.Contains(search));
        }

        var employees = await employeesQuery.ToListAsync();
        var employeeIds = employees.Select(e => e.Id).ToList();

        // Tính số ngày chấm công từ AttendanceCheckIns và AttendanceCheckOuts
        var attendanceData = await _context.AttendanceCheckIns
            .Where(ci => employeeIds.Contains(ci.EmployeeId) &&
                        ci.AttendanceDate.Month == month &&
                        ci.AttendanceDate.Year == year)
            .Join(_context.AttendanceCheckOuts,
                ci => new { ci.EmployeeId, ci.AttendanceDate, ci.Shift },
                co => new { co.EmployeeId, co.AttendanceDate, co.Shift },
                (ci, co) => new
                {
                    ci.EmployeeId,
                    ci.AttendanceDate,
                    ci.Shift
                })
            .ToListAsync();

        var workDaysDict = attendanceData
            .GroupBy(x => new { x.EmployeeId, x.AttendanceDate })
            .Select(g => new { g.Key.EmployeeId, g.Key.AttendanceDate })
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );

        // Tính giờ tăng ca từ OvertimeRecord
        var overtimeRecords = await _context.OvertimeRecords
            .Where(o => employeeIds.Contains(o.EmployeeId) &&
                        o.Date.Month == month &&
                        o.Date.Year == year &&
                        o.IsApproved == true)
            .ToListAsync(); // Chuyển sang danh sách để xử lý phía client

        var overtimeData = overtimeRecords
            .GroupBy(o => o.EmployeeId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(o => o.TotalHours)
            );

        // Lấy danh sách lương hiện có
        var existingSalariesList = await _context.Salaries
            .Where(s => employeeIds.Contains(s.EmployeeId) &&
                        s.StartDate.HasValue &&
                        s.StartDate.Value.Month == month &&
                        s.StartDate.Value.Year == year)
            .GroupBy(s => s.EmployeeId)
            .Select(g => g.OrderByDescending(s => s.StartDate).First())
            .ToListAsync();

        var existingSalaries = existingSalariesList.ToDictionary(s => s.EmployeeId, s => s);

        var salaryRecords = new List<object>();
        decimal overtimeRate = 50000; // Giả định: 50,000 VNĐ/giờ tăng ca

        foreach (var employee in employees)
        {
            int totalWorkDays = workDaysDict.ContainsKey(employee.Id) ? workDaysDict[employee.Id] : 0;
            decimal totalOvertimeHours = overtimeData.ContainsKey(employee.Id) ? overtimeData[employee.Id] : 0;
            decimal overtimePay = totalOvertimeHours * overtimeRate;
            Salary salaryRecord;

            if (existingSalaries.TryGetValue(employee.Id, out salaryRecord))
            {
                salaryRecord.FinalSalary = (int)((salaryRecord.FixedSalary ?? 0) * totalWorkDays) + (int)overtimePay;
                salaryRecord.BonusSalary = (int)overtimePay; // Chỉ tính overtimePay
            }
            else
            {
                salaryRecord = new Salary
                {
                    EmployeeId = employee.Id,
                    FixedSalary = employee.FixedSalary,
                    StartDate = startDate,
                    EndDate = endDate,
                    BonusSalary = (int)overtimePay, // Chỉ tính overtimePay
                    FinalSalary = (int)((employee.FixedSalary ?? 0) * totalWorkDays) + (int)overtimePay,
                    Status = "Pending"
                };
                _context.Salaries.Add(salaryRecord);
            }

            salaryRecords.Add(new
            {
                salaryRecord.EmployeeId,
                EmployeeName = employee.FullName,
                Phone = employee.PhoneNumber,
                FixedSalary = salaryRecord.FixedSalary ?? 0,
                TotalWorkDays = totalWorkDays,
                TotalOvertimeHours = totalOvertimeHours,
                OvertimePay = (int)overtimePay,
                TotalSalary = salaryRecord.FinalSalary ?? 0,
                IdentityNumber = employee.IdentityNumber,
                CurrentAddress = employee.CurrentAddress,
                Hometown = employee.Hometown
            });
        }

        await _context.SaveChangesAsync();
        return Ok(salaryRecords);
    }

    [HttpGet("details")]
    public async Task<IActionResult> GetPayrollDetails(
        [FromQuery] int employeeId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var salaryRecord = await _context.Salaries
            .Where(s => s.EmployeeId == employeeId &&
                       s.StartDate <= endDate &&
                       (s.EndDate == null || s.EndDate >= startDate))
            .OrderByDescending(s => s.StartDate)
            .Select(s => new
            {
                s.Id,
                s.EmployeeId,
                s.FixedSalary,
                s.BonusSalary,
                s.FinalSalary,
                s.StartDate,
                s.Status,
                Employee = new
                {
                    s.Employee.Id,
                    s.Employee.FullName,
                    s.Employee.PhoneNumber,
                    s.Employee.BirthDate,
                    s.Employee.BranchId,
                    s.Employee.CurrentAddress,
                    s.Employee.FixedSalary,
                    s.Employee.Gender,
                    s.Employee.Hometown,
                    s.Employee.IdentityNumber,
                    s.Employee.Image,
                    s.Employee.StartDate,
                }
            })
            .FirstOrDefaultAsync();

        var attendanceData = await _context.AttendanceCheckIns
            .Where(ci => ci.EmployeeId == employeeId &&
                        ci.AttendanceDate.Month == month &&
                        ci.AttendanceDate.Year == year)
            .Join(_context.AttendanceCheckOuts,
                ci => new { ci.EmployeeId, ci.AttendanceDate, ci.Shift },
                co => new { co.EmployeeId, co.AttendanceDate, co.Shift },
                (ci, co) => new { ci.AttendanceDate })
            .ToListAsync();

        int totalWorkDays = attendanceData
            .GroupBy(x => x.AttendanceDate)
            .Count();

        // Lấy giờ tăng ca từ OvertimeRecord
        decimal totalOvertimeHours = await _context.OvertimeRecords
            .Where(o => o.EmployeeId == employeeId &&
                        o.Date.Month == month &&
                        o.Date.Year == year &&
                        o.IsApproved == true) // Chỉ lấy các bản ghi đã duyệt
            .SumAsync(o => o.TotalHours);

        decimal overtimeRate = 50000; // 50,000 VNĐ/giờ tăng ca
        decimal overtimePay = totalOvertimeHours * overtimeRate;

        if (salaryRecord == null)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            return Ok(new
            {
                EmployeeId = employeeId,
                EmployeeName = employee.FullName,
                Phone = employee.PhoneNumber,
                FixedSalary = employee.FixedSalary ?? 0,
                TotalWorkDays = totalWorkDays,
                TotalOvertimeHours = totalOvertimeHours,
                OvertimePay = (int)overtimePay,
                TotalSalary = (int)((employee.FixedSalary ?? 0) * totalWorkDays) + (int)overtimePay,
                IdentityNumber = employee.IdentityNumber,
                CurrentAddress = employee.CurrentAddress,
                Hometown = employee.Hometown,
                PaymentHistory = new List<object>()
            });
        }

        var paymentHistory = await _context.SalaryPaymentHistories
            .Where(p => p.EmployeeId == employeeId &&
                       p.PaymentDate.HasValue &&
                       p.PaymentDate.Value.Month == month &&
                       p.PaymentDate.Value.Year == year)
            .Select(p => new
            {
                p.PaymentDate,
                p.PaidAmount
            })
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        var result = new
        {
            EmployeeId = salaryRecord.EmployeeId,
            EmployeeName = salaryRecord.Employee.FullName,
            Phone = salaryRecord.Employee.PhoneNumber,
            FixedSalary = salaryRecord.FixedSalary ?? 0,
            TotalWorkDays = totalWorkDays,
            TotalOvertimeHours = totalOvertimeHours,
            OvertimePay = (int)overtimePay,
            TotalSalary = salaryRecord.FinalSalary ??
                         ((salaryRecord.FixedSalary ?? 0) * totalWorkDays + (int)overtimePay),
            IdentityNumber = salaryRecord.Employee.IdentityNumber,
            CurrentAddress = salaryRecord.Employee.CurrentAddress,
            Hometown = salaryRecord.Employee.Hometown,
            PaymentHistory = paymentHistory
        };

        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportPayroll([FromQuery] int month, [FromQuery] int year)
    {
        var payrollList = await _context.Salaries
            .Include(s => s.Employee)
            .Where(s => s.StartDate.HasValue &&
                       s.StartDate.Value.Month == month &&
                       s.StartDate.Value.Year == year)
            .AsNoTracking()
            .Select(s => new
            {
                s.EmployeeId,
                s.Employee.FullName,
                FixedSalary = s.FixedSalary ?? 0,
                TotalWorkDays = _context.AttendanceCheckIns
                    .Where(ci => ci.EmployeeId == s.EmployeeId &&
                                ci.AttendanceDate.Month == month &&
                                ci.AttendanceDate.Year == year)
                    .Join(_context.AttendanceCheckOuts,
                        ci => new { ci.EmployeeId, ci.AttendanceDate, ci.Shift },
                        co => new { co.EmployeeId, co.AttendanceDate, co.Shift },
                        (ci, co) => new { ci.AttendanceDate })
                    .Distinct()
                    .Count(),
                TotalOvertimeHours = _context.OvertimeRecords
                    .Where(o => o.EmployeeId == s.EmployeeId &&
                                o.Date.Month == month &&
                                o.Date.Year == year &&
                                o.IsApproved == true) // Chỉ lấy các bản ghi đã duyệt
                    .Sum(o => o.TotalHours)
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Payroll");
        var headers = new string[]
        {
            "Employee ID", "Full Name", "Fixed Salary", "Total Work Days",
            "Overtime Hours", "Overtime Pay", "Total Salary"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        int row = 2;
        decimal overtimeRate = 50000; // 50,000 VNĐ/giờ tăng ca
        foreach (var p in payrollList)
        {
            decimal overtimePay = p.TotalOvertimeHours * overtimeRate;
            worksheet.Cell(row, 1).Value = p.EmployeeId;
            worksheet.Cell(row, 2).Value = p.FullName;
            worksheet.Cell(row, 3).Value = p.FixedSalary;
            worksheet.Cell(row, 4).Value = p.TotalWorkDays;
            worksheet.Cell(row, 5).Value = p.TotalOvertimeHours;
            worksheet.Cell(row, 6).Value = overtimePay;
            worksheet.Cell(row, 7).Value = (p.FixedSalary * p.TotalWorkDays) + overtimePay;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Payroll_{month}_{year}.xlsx");
    }

    [HttpPut("update-salary")]
    public async Task<IActionResult> UpdateSalaryByEmployeeIdAndMonth([FromBody] SalaryDTO request)
    {
        if (request == null || request.EmployeeId <= 0 || request.StartDate == null)
        {
            return BadRequest("Dữ liệu yêu cầu không hợp lệ.");
        }

        int month = request.StartDate.Value.Month;
        int year = request.StartDate.Value.Year;

        var salaryRecord = await _context.Salaries
            .Include(s => s.Employee)
            .FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId &&
                                     s.StartDate.HasValue &&
                                     s.StartDate.Value.Month == month &&
                                     s.StartDate.Value.Year == year);

        if (salaryRecord == null)
        {
            return NotFound("Không tìm thấy bảng lương của nhân viên trong tháng và năm đã cho.");
        }

        // Kiểm tra xem lương đã được thanh toán chưa
        bool hasReceivedSalary = await _context.SalaryPaymentHistories
            .AnyAsync(p => p.EmployeeId == request.EmployeeId &&
                          p.PaymentDate.HasValue &&
                          p.PaymentDate.Value.Month == month &&
                          p.PaymentDate.Value.Year == year);

        if (hasReceivedSalary && request.FixedSalary.HasValue && request.FixedSalary != salaryRecord.FixedSalary)
        {
            return BadRequest("Không thể cập nhật FixedSalary vì lương đã được thanh toán.");
        }

        if (request.Status == "Done" && !hasReceivedSalary)
        {
            return BadRequest("Nhân viên chưa nhận lương, không thể cập nhật trạng thái thành 'Done'.");
        }

        // Tính số ngày làm việc
        int totalWorkDays = await _context.AttendanceCheckIns
            .Where(ci => ci.EmployeeId == request.EmployeeId &&
                        ci.AttendanceDate.Month == month &&
                        ci.AttendanceDate.Year == year)
            .Join(_context.AttendanceCheckOuts,
                ci => new { ci.EmployeeId, ci.AttendanceDate, ci.Shift },
                co => new { co.EmployeeId, co.AttendanceDate, co.Shift },
                (ci, co) => new { ci.AttendanceDate })
            .Distinct()
            .CountAsync();

        // Tính giờ tăng ca từ OvertimeRecord
        decimal totalOvertimeHours = await _context.OvertimeRecords
            .Where(o => o.EmployeeId == request.EmployeeId &&
                        o.Date.Month == month &&
                        o.Date.Year == year &&
                        o.IsApproved == true) // Chỉ lấy các bản ghi đã duyệt
            .SumAsync(o => o.TotalHours);

        decimal overtimeRate = 50000; // 50,000 VNĐ/giờ tăng ca
        decimal overtimePay = totalOvertimeHours * overtimeRate;

        // Cập nhật FixedSalary
        if (request.FixedSalary.HasValue)
        {
            if (request.FixedSalary < 0)
            {
                return BadRequest("FixedSalary không thể là số âm.");
            }
            salaryRecord.FixedSalary = request.FixedSalary.Value;
        }
        else if (salaryRecord.FixedSalary == null)
        {
            // Nếu không có giá trị trong request và FixedSalary hiện tại là null, lấy từ Employee
            salaryRecord.FixedSalary = salaryRecord.Employee?.FixedSalary ?? 0;
        }

        // Cập nhật các trường khác (không tính bonus từ shifts)
        salaryRecord.BonusSalary = (int)overtimePay; // Chỉ tính overtimePay
        salaryRecord.Status = request.Status;
        salaryRecord.FinalSalary = (int)((salaryRecord.FixedSalary ?? 0) * totalWorkDays) + (int)overtimePay;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            salaryRecord.EmployeeId,
            EmployeeName = salaryRecord.Employee?.FullName ?? "Không xác định",
            Month = month,
            Year = year,
            FixedSalary = salaryRecord.FixedSalary,
            TotalOvertimeHours = totalOvertimeHours,
            OvertimePay = (int)overtimePay,
            TotalSalary = salaryRecord.FinalSalary,
            Status = salaryRecord.Status
        });
    }

    private decimal CalculateBonus(int totalShifts)
    {
        // Giữ lại hàm này để tương thích với các phần khác nếu cần, nhưng không sử dụng trong tính lương
        if (totalShifts > 10)
            return 1000000; // 1,000,000 VNĐ
        if (totalShifts > 5)
            return 500000;  // 500,000 VNĐ
        if (totalShifts > 3)
            return 200000;  // 200,000 VNĐ
        return 0;
    }
}
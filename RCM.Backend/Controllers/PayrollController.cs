using ClosedXML.Excel;
using DataLayerObject.Models;
using DocumentFormat.OpenXml.Bibliography;
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
        var endDate = startDate.AddMonths(1).AddDays(-1); // Ngày cuối tháng

        // Kiểm tra xem đã có dữ liệu lương cho tháng này chưa
        bool payrollExists = await _context.Salaries
            .AnyAsync(s => s.StartDate != null &&
                           s.StartDate.Value.Month == month &&
                           s.StartDate.Value.Year == year);

        // Lấy danh sách nhân viên có thể lọc theo search
        var employeesQuery = _context.Employees.AsNoTracking();
        if (!string.IsNullOrEmpty(search))
        {
            employeesQuery = employeesQuery.Where(e => e.FullName.Contains(search) || e.PhoneNumber.Contains(search));
        }

        var employees = await employeesQuery.ToListAsync();
        var employeeIds = employees.Select(e => e.Id).ToList();

        // Lấy số ngày làm việc của tất cả nhân viên trong tháng
        var attendanceCounts = await _context.AttendanceHis
            .Where(a => employeeIds.Contains(a.EmployeeId) &&
                        a.AttendanceDate.Month == month &&
                        a.AttendanceDate.Year == year)
            .GroupBy(a => a.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, TotalWorkDays = g.Count() })
            .ToListAsync();

        var attendanceDict = new Dictionary<int, int>();
        foreach (var item in attendanceCounts)
        {
            attendanceDict[item.EmployeeId] = item.TotalWorkDays;
        }

        // Lấy danh sách lương đã có trong tháng
        var existingSalariesList = await _context.Salaries
            .Where(s => employeeIds.Contains(s.EmployeeId) &&
                        s.StartDate != null &&
                        s.StartDate.Value.Month == month &&
                        s.StartDate.Value.Year == year)
            .ToListAsync();

        var existingSalaries = new Dictionary<int, Salary>();
        foreach (var salary in existingSalariesList)
        {
            if (!existingSalaries.ContainsKey(salary.EmployeeId))
            {
                existingSalaries[salary.EmployeeId] = salary;
            }
        }

        var salaryRecords = new List<object>();

        foreach (var employee in employees)
        {
            int totalWorkDays = attendanceDict.ContainsKey(employee.Id) ? attendanceDict[employee.Id] : 0;
            Salary salaryRecord;

            if (existingSalaries.TryGetValue(employee.Id, out salaryRecord))
            {
                // Nếu đã có, cập nhật FinalSalary
                decimal dailySalary = (salaryRecord.FixedSalary ?? 0) / 26;
                salaryRecord.FinalSalary = (int)((dailySalary * totalWorkDays) + (salaryRecord.BonusSalary ?? 0));

                if (totalWorkDays > 26)
                {
                    salaryRecord.BonusSalary = (salaryRecord.BonusSalary ?? 0) + 500000;
                    salaryRecord.FinalSalary += 500000;
                }
            }
            else
            {
                // Nếu chưa có, thêm mới cho nhân viên này
                salaryRecord = new Salary
                {
                    EmployeeId = employee.Id,
                    FixedSalary = employee.FixedSalary,
                    StartDate = startDate,
                    EndDate = endDate,
                    BonusSalary = totalWorkDays > 26 ? 500000 : 0, // Nếu làm trên 26 ngày thì thưởng 500,000
                    FinalSalary = (int)(((employee.FixedSalary ?? 0) / 26) * totalWorkDays) + (totalWorkDays > 26 ? 500000 : 0)
                };

                _context.Salaries.Add(salaryRecord);
            }

            // Lưu dữ liệu vào danh sách kết quả
            salaryRecords.Add(new
            {
                salaryRecord.EmployeeId,
                EmployeeName = employee.FullName,
                Phone = employee.PhoneNumber,
                FixedSalary = salaryRecord.FixedSalary ?? 0,
                TotalWorkDays = totalWorkDays,
                DailySalary = (salaryRecord.FixedSalary ?? 0) / 26,
                BonusSalary = salaryRecord.BonusSalary ?? 0,
                TotalSalary = salaryRecord.FinalSalary ?? 0,
                IdentityNumber = employee.IdentityNumber,
                CurrentAddress = employee.CurrentAddress,
                Hometown = employee.Hometown

            });
        }

        // Lưu tất cả thay đổi vào database
        await _context.SaveChangesAsync();

        return Ok(salaryRecords);
    }

    /// <summary>
    /// Lấy chi tiết Payroll theo tháng, năm
    /// </summary>
    [HttpGet("details")]
    public async Task<IActionResult> GetPayrollDetails(
        [FromQuery] int employeeId,
        [FromQuery] int month,
        [FromQuery] int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Truy vấn Salary với điều kiện hợp đồng lương hợp lệ
        var salaryRecord = await _context.Salaries
        .Where(s => s.EmployeeId == employeeId &&
                    s.StartDate <= endDate && // endDate là ngày cuối cùng của tháng
                    (s.EndDate == null || s.EndDate >= startDate)) // startDate là ngày đầu tiên của tháng
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
                s.Employee.ActiveStatus,
                s.Employee.BirthDate,
                s.Employee.BranchId,
                s.Employee.CurrentAddress,
                s.Employee.FixedSalary,
                s.Employee.Gender,
                s.Employee.Hometown,
                s.Employee.IdentityNumber,
                s.Employee.Image,
                s.Employee.StartDate,
                s.Employee.WorkShiftId
            }
        })
        .FirstOrDefaultAsync();

        // Nếu không tìm thấy, tạo một bản ghi mặc định
        if (salaryRecord == null)
        {
            return Ok(new
            {
                EmployeeId = employeeId,
                FixedSalary = 0,
                BonusSalary = 0,
                FinalSalary = 0,
                StartDate = startDate,
                Employee = new
                {
                    FullName = "Không xác định",
                    PhoneNumber = "N/A",
                    ActiveStatus = false
                }
            });
        }



        var totalWorkDays = await _context.AttendanceHis
            .CountAsync(a => a.EmployeeId == employeeId &&
                             a.AttendanceDate.Month == month &&
                             a.AttendanceDate.Year == year);

        // Lấy lịch sử thanh toán lương
        var paymentHistory = await _context.SalaryPaymentHistories
            .Where(p => p.EmployeeId == employeeId &&
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
            DailySalary = (salaryRecord.FixedSalary ?? 0) / 26,
            BonusSalary = salaryRecord.BonusSalary ?? 0,
            TotalSalary = salaryRecord.FinalSalary ??
                          (((salaryRecord.FixedSalary ?? 0) / 30) * totalWorkDays
                          + (salaryRecord.BonusSalary ?? 0)),
                          IdentityNumber = salaryRecord.Employee.IdentityNumber,
                          CurrentAddress = salaryRecord.Employee.CurrentAddress,
                          Hometown = salaryRecord.Employee.Hometown,
            PaymentHistory = paymentHistory
        };

        return Ok(salaryRecord);
    }
  

    [HttpGet("export")]
    public async Task<IActionResult> ExportPayroll([FromQuery] int month, [FromQuery] int year)
    {
        var payrollList = await _context.Salaries
            .Include(s => s.Employee)
            .AsNoTracking()
            .Select(s => new
            {
                s.EmployeeId,
                s.Employee.FullName,
                FixedSalary = s.FixedSalary ?? 0,
                BonusSalary = s.BonusSalary ?? 0,
                TotalWorkDays = _context.AttendanceHis
                    .Count(a => a.EmployeeId == s.EmployeeId &&
                                a.AttendanceDate.Month == month &&
                                a.AttendanceDate.Year == year)
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Payroll");
        var headers = new string[]
        {
        "Employee ID", "Full Name", "Fixed Salary", "Total Work Days", "Daily Salary", "Bonus Salary", "Penalty", "Total Salary"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        int row = 2;
        foreach (var p in payrollList)
        {
            worksheet.Cell(row, 1).Value = p.EmployeeId;
            worksheet.Cell(row, 2).Value = p.FullName;
            worksheet.Cell(row, 3).Value = p.FixedSalary;
            worksheet.Cell(row, 4).Value = p.TotalWorkDays;
            worksheet.Cell(row, 5).Value = p.FixedSalary / 30;
            worksheet.Cell(row, 6).Value = p.BonusSalary;
            worksheet.Cell(row, 7).Value = (p.FixedSalary / 30) * p.TotalWorkDays + p.BonusSalary;
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
            .Include(s => s.Employee) // Load Employee nếu cần
            .FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId &&
                                      s.StartDate.HasValue &&
                                      s.StartDate.Value.Month == month &&
                                      s.StartDate.Value.Year == year);

        if (salaryRecord == null)
        {
            return NotFound("Không tìm thấy bảng lương của nhân viên trong tháng và năm đã cho.");
        }

        // Kiểm tra xem nhân viên đã nhận lương hay chưa
        bool hasReceivedSalary = await _context.SalaryPaymentHistories
            .AnyAsync(p => p.EmployeeId == request.EmployeeId &&
                           p.PaymentDate.Value.Month == month &&
                           p.PaymentDate.Value.Year == year);

        // Kiểm tra xem nhân viên đã nộp phạt hay chưa
        //bool hasPaidPenalty = await _context.PenaltyPayments
        //    .AnyAsync(p => p.EmployeeId == request.EmployeeId &&
        //                   p.PaymentDate.Value.Month == month &&
        //                   p.PaymentDate.Value.Year == year);

        // Nếu nhân viên chưa nhận lương và chưa nộp phạt, không cho cập nhật trạng thái Done
        if (request.Status == "Done" && !hasReceivedSalary)
        {
            return BadRequest("Nhân viên chưa nhận lương, không thể cập nhật trạng thái thành 'Done'.");
        }

        // Cập nhật thông tin lương
        salaryRecord.FixedSalary = request.FixedSalary ?? 0;
        salaryRecord.BonusSalary = request.BonusSalary ?? 0;
        salaryRecord.Status = request.Status; // Cập nhật trạng thái (Done, Pending,...)

        // Tính toán lại tổng lương
        int totalWorkDays = await _context.AttendanceHis
            .Where(a => a.EmployeeId == request.EmployeeId &&
                        a.AttendanceDate.Month == month &&
                        a.AttendanceDate.Year == year)
            .CountAsync();

        decimal dailySalary = (decimal)salaryRecord.FixedSalary / 30;
        salaryRecord.FinalSalary = (int)((dailySalary * totalWorkDays) + salaryRecord.BonusSalary);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            salaryRecord.EmployeeId,
            EmployeeName = salaryRecord.Employee?.FullName ?? "Không xác định",
            Month = month,
            Year = year,
            FixedSalary = salaryRecord.FixedSalary,
            BonusSalary = salaryRecord.BonusSalary,
            TotalSalary = salaryRecord.FinalSalary,
            Status = salaryRecord.Status
        });
    }



}

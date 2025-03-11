using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCM.Backend.DTO;
using RCM.Backend.Models;
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
            .ToDictionaryAsync(x => x.EmployeeId, x => x.TotalWorkDays);

        // Lấy danh sách lương đã có trong tháng
        var existingSalaries = await _context.Salaries
            .Where(s => employeeIds.Contains(s.EmployeeId) &&
                        s.StartDate != null &&
                        s.StartDate.Value.Month == month &&
                        s.StartDate.Value.Year == year)
            .ToDictionaryAsync(s => s.EmployeeId);

        var salaryRecords = new List<object>();

        foreach (var employee in employees)
        {
            int totalWorkDays = attendanceCounts.ContainsKey(employee.Id) ? attendanceCounts[employee.Id] : 0;
            Salary salaryRecord;

            if (existingSalaries.TryGetValue(employee.Id, out salaryRecord))
            {
                // Nếu đã có, cập nhật FinalSalary
                decimal dailySalary = (salaryRecord.FixedSalary ?? 0) / 30;
                salaryRecord.FinalSalary = (int)((dailySalary * totalWorkDays)
                                                + (salaryRecord.BonusSalary ?? 0)
                                                - (salaryRecord.Penalty ?? 0));
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
                    BonusSalary = 0,
                    Penalty = 0,
                    FinalSalary = (int)(((employee.FixedSalary ?? 0) / 30) * totalWorkDays)
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
                DailySalary = (salaryRecord.FixedSalary ?? 0) / 30,
                BonusSalary = salaryRecord.BonusSalary ?? 0,
                Penalty = salaryRecord.Penalty ?? 0,
                TotalSalary = salaryRecord.FinalSalary ?? 0
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
        var endDate = startDate.AddMonths(1).AddDays(-1); // Ngày cuối tháng

        var salaryRecord = await _context.Salaries
            .Include(s => s.Employee)
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId &&
                        s.StartDate <= endDate &&
                        (s.EndDate == null || s.EndDate >= startDate)) // Lọc hợp đồng lương hợp lệ
            .OrderByDescending(s => s.StartDate) // Lấy hợp đồng gần nhất
            .FirstOrDefaultAsync();

        if (salaryRecord == null)
        {
            return NotFound($"Không tìm thấy bảng lương cho nhân viên {employeeId} trong tháng {month}/{year}");
        }

        var totalWorkDays = await _context.AttendanceHis
            .CountAsync(a => a.EmployeeId == employeeId &&
                             a.AttendanceDate.Month == month &&
                             a.AttendanceDate.Year == year);

        var result = new
        {
            salaryRecord.EmployeeId,
            EmployeeName = salaryRecord.Employee.FullName,
            Phone = salaryRecord.Employee.PhoneNumber,
            FixedSalary = salaryRecord.FixedSalary ?? 0,
            TotalWorkDays = totalWorkDays,
            DailySalary = (salaryRecord.FixedSalary ?? 0) / 30,
            BonusSalary = salaryRecord.BonusSalary ?? 0,
            Penalty = salaryRecord.Penalty ?? 0,
            TotalSalary = salaryRecord.FinalSalary ??
                          (((salaryRecord.FixedSalary ?? 0) / 30) * totalWorkDays
                          + (salaryRecord.BonusSalary ?? 0) - (salaryRecord.Penalty ?? 0))
        };

        return Ok(result);
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
                Penalty = s.Penalty ?? 0,
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
            worksheet.Cell(row, 7).Value = p.Penalty;
            worksheet.Cell(row, 8).Value = (p.FixedSalary / 30) * p.TotalWorkDays + p.BonusSalary - p.Penalty;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Payroll_{month}_{year}.xlsx");
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetPayrollList(
     [FromQuery] int? month,
     [FromQuery] int? year,
     [FromQuery] string? search)
    {
        if (!month.HasValue || !year.HasValue)
        {
            return BadRequest("Month and year are required.");
        }

        var startDate = new DateTime(year.Value, month.Value, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1); // Lấy ngày cuối tháng

        var query = _context.Salaries
            .Include(s => s.Employee)
            .AsNoTracking()
            .Where(s =>
                s.StartDate <= endDate &&
                (s.EndDate == null || s.EndDate >= startDate)) // Lọc theo hợp đồng lương hợp lệ
            .Select(s => new
            {
                s.EmployeeId,
                EmployeeName = s.Employee.FullName,
                Phone = s.Employee.PhoneNumber,
                FixedSalary = s.FixedSalary ?? 0,
                BonusSalary = s.BonusSalary ?? 0,
                Penalty = s.Penalty ?? 0,
                FinalSalary = s.FinalSalary,
                TotalWorkDays = _context.AttendanceHis
                    .Count(a => a.EmployeeId == s.EmployeeId &&
                                a.AttendanceDate.Month == month &&
                                a.AttendanceDate.Year == year)
            });

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.EmployeeName.Contains(search) || e.EmployeeId.ToString() == search);
        }

        var payrollList = await query.ToListAsync();

        var result = payrollList.Select(p => new
        {
            p.EmployeeId,
            p.EmployeeName,
            p.Phone,
            FixedSalary = p.FixedSalary,
            TotalWorkDays = p.TotalWorkDays,
            DailySalary = p.FixedSalary / 30,
            BonusSalary = p.BonusSalary,
            Penalty = p.Penalty,
            TotalSalary = p.FinalSalary ?? ((p.FixedSalary / 30) * p.TotalWorkDays + p.BonusSalary - p.Penalty) // Nếu FinalSalary có sẵn thì dùng, nếu không thì tự tính
        });

        return Ok(result);
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
            .Include(s => s.Employee) // Đảm bảo Employee được load nếu cần dùng
            .FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId &&
                                      s.StartDate.HasValue &&
                                      s.StartDate.Value.Month == month &&
                                      s.StartDate.Value.Year == year);

        if (salaryRecord == null)
        {
            return NotFound("Không tìm thấy bảng lương của nhân viên trong tháng và năm đã cho.");
        }

        // Đảm bảo không có giá trị null khi tính toán
        salaryRecord.FixedSalary = request.FixedSalary ?? 0;
        salaryRecord.BonusSalary = request.BonusSalary ?? 0;
        salaryRecord.Penalty = request.Penalty ?? 0;

        // Tính toán lại tổng lương
        int totalWorkDays = await _context.AttendanceHis
            .Where(a => a.EmployeeId == request.EmployeeId &&
                        a.AttendanceDate.Month == month &&
                        a.AttendanceDate.Year == year)
            .CountAsync();

        decimal dailySalary = (decimal)salaryRecord.FixedSalary / 30; // Chắc chắn không bị null
        salaryRecord.FinalSalary = (int)((dailySalary * totalWorkDays) + salaryRecord.BonusSalary - salaryRecord.Penalty);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            salaryRecord.EmployeeId,
            EmployeeName = salaryRecord.Employee?.FullName ?? "Không xác định", // Tránh null khi Employee không tồn tại
            Month = month,
            Year = year,
            FixedSalary = salaryRecord.FixedSalary,
            BonusSalary = salaryRecord.BonusSalary,
            Penalty = salaryRecord.Penalty,
            TotalSalary = salaryRecord.FinalSalary
        });
    }



}

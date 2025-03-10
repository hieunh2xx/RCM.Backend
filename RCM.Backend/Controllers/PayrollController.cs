using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

}

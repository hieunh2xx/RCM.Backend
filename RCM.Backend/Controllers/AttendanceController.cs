using DataLayerObject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RCM.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly RetailChainContext _context;
        private readonly IConfiguration _configuration;

        public AttendanceController(RetailChainContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("CheckIn")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
        {
            if (request == null || request.EmployeeId <= 0)
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            var employee = await _context.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            var morningShiftStart = new TimeSpan(8, 0, 0);    // 8:00
            var morningShiftEnd = new TimeSpan(15, 0, 0);     // 15:00
            var afternoonShiftStart = new TimeSpan(15, 0, 0); // 15:00
            var afternoonShiftEnd = new TimeSpan(22, 0, 0);   // 22:00
            var gracePeriod = new TimeSpan(0, 15, 0);         // 15 phút ân hạn

            string shift;
            bool isOnTime;
            TimeSpan shiftStart;

            // Xác định ca dựa trên giờ hiện tại
            if (currentTime >= morningShiftStart && currentTime < afternoonShiftStart)
            {
                shift = "Ca sáng";
                shiftStart = morningShiftStart;
                isOnTime = currentTime <= (morningShiftStart + gracePeriod);
            }
            else if (currentTime >= afternoonShiftStart && currentTime <= afternoonShiftEnd)
            {
                shift = "Ca chiều";
                shiftStart = afternoonShiftStart;
                isOnTime = currentTime <= (afternoonShiftStart + gracePeriod);
            }
            else
            {
                return BadRequest("Ngoài giờ check-in cho phép (8:00-22:00).");
            }

            // Kiểm tra xem đã check-in cho ca này chưa
            var existingCheckIn = await _context.AttendanceCheckIns
                .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId
                    && a.AttendanceDate == now.Date
                    && a.Shift == shift);

            if (existingCheckIn != null)
            {
                return BadRequest($"Nhân viên đã check-in cho {shift} hôm nay.");
            }

            // Kiểm tra xem có check-in ca khác trong ngày không
            var otherShiftCheckIn = await _context.AttendanceCheckIns
                .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId
                    && a.AttendanceDate == now.Date
                    && a.Shift != shift);

            var checkIn = new AttendanceCheckIn
            {
                EmployeeId = request.EmployeeId,
                AttendanceDate = now.Date,
                Shift = shift,
                CheckInTime = now
            };

            _context.AttendanceCheckIns.Add(checkIn);

            // Nếu đã có check-in ca khác trong ngày, thêm vào OvertimeRecord
            if (otherShiftCheckIn != null)
            {
                var overtimeRecord = new OvertimeRecord
                {
                    EmployeeId = request.EmployeeId,
                    Date = now.Date,
                    StartTime = currentTime,
                    EndTime = TimeSpan.Zero, // Sẽ cập nhật khi check-out
                    TotalHours = 0, // Sẽ cập nhật khi check-out
                    Reason = $"Check-in lệch ca - {shift}",
                    IsApproved = null // Chưa duyệt
                };
                _context.OvertimeRecords.Add(overtimeRecord);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Check-in thành công.",
                Shift = shift,
                Status = isOnTime ? "Đúng giờ" : "Đi muộn",
                TimeCheckIn = now.ToString("dd/MM/yyyy HH:mm:ss"),
                Overtime = otherShiftCheckIn != null ? "Đã ghi nhận tăng ca (chưa hoàn tất)" : "Không tăng ca"
            });
        }

        [HttpPost("CheckOut")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
        {
            if (request == null || request.EmployeeId <= 0)
            {
                return BadRequest("Invalid request.");
            }

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            var morningShiftEnd = new TimeSpan(15, 0, 0);    // 15:00
            var afternoonShiftEnd = new TimeSpan(22, 0, 0);  // 22:00

            // Tìm bản ghi check-in gần nhất trong ngày
            var checkIn = await _context.AttendanceCheckIns
                .Where(a => a.EmployeeId == request.EmployeeId
                    && a.AttendanceDate == now.Date)
                .OrderByDescending(a => a.CheckInTime)
                .FirstOrDefaultAsync();

            if (checkIn == null)
            {
                return BadRequest("No check-in record found for today.");
            }

            // Kiểm tra xem đã check-out cho ca này chưa
            var existingCheckOut = await _context.AttendanceCheckOuts
                .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId
                    && a.AttendanceDate == now.Date
                    && a.Shift == checkIn.Shift);

            if (existingCheckOut != null)
            {
                return BadRequest($"Employee has already checked out for {checkIn.Shift} today.");
            }

            bool isValidCheckout;
            decimal overtimeHours = 0;
            TimeSpan shiftEnd = checkIn.Shift == "Ca sáng" ? morningShiftEnd : afternoonShiftEnd;

            isValidCheckout = currentTime >= shiftEnd;
            if (isValidCheckout && currentTime > shiftEnd)
            {
                overtimeHours = (decimal)(currentTime - shiftEnd).TotalHours;
            }

            if (!isValidCheckout)
            {
                return BadRequest($"Check-out không hợp lệ. Phải sau {(checkIn.Shift == "Ca sáng" ? "15:00" : "22:00")}");
            }

            var checkOut = new AttendanceCheckOut
            {
                EmployeeId = request.EmployeeId,
                AttendanceDate = now.Date,
                Shift = checkIn.Shift,
                CheckOutTime = now
            };

            _context.AttendanceCheckOuts.Add(checkOut);

            // Kiểm tra xem có OvertimeRecord chưa hoàn tất từ check-in lệch ca không
            var existingOvertime = await _context.OvertimeRecords
                .FirstOrDefaultAsync(o => o.EmployeeId == request.EmployeeId
                    && o.Date == now.Date
                    && o.EndTime == TimeSpan.Zero);

            if (existingOvertime != null)
            {
                // Cập nhật OvertimeRecord từ check-in lệch ca
                existingOvertime.EndTime = currentTime;
                existingOvertime.TotalHours = (decimal)(currentTime - existingOvertime.StartTime).TotalHours;
            }
            else if (overtimeHours > 0)
            {
                // Thêm OvertimeRecord mới nếu check-out vượt giờ ca chuẩn
                var overtimeRecord = new OvertimeRecord
                {
                    EmployeeId = request.EmployeeId,
                    Date = now.Date,
                    StartTime = shiftEnd, // Bắt đầu tăng ca từ giờ kết thúc ca chuẩn
                    EndTime = currentTime,
                    TotalHours = overtimeHours,
                    Reason = $"Check-out vượt giờ - {checkIn.Shift}",
                    IsApproved = null // Chưa duyệt
                };
                _context.OvertimeRecords.Add(overtimeRecord);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Check-out thành công.",
                Ca_làm = checkIn.Shift,
                CheckOutTime = now.ToString("dd/MM/yyyy HH:mm:ss"),
                OvertimeHours = overtimeHours > 0 || existingOvertime != null
                    ? $"{(existingOvertime?.TotalHours ?? overtimeHours):F2} giờ tăng ca"
                    : "Không tăng ca"
            });
        }

        [HttpGet("AttendanceDetail")]
        public async Task<IActionResult> GetAttendance([FromQuery] int employeeId)
        {
            if (employeeId <= 0)
            {
                return BadRequest("Invalid Employee ID.");
            }

            var morningShiftStart = new TimeSpan(8, 0, 0);
            var afternoonShiftStart = new TimeSpan(15, 0, 0);
            var gracePeriod = new TimeSpan(0, 15, 0);

            var attendanceRecords = await _context.AttendanceCheckIns
                .Where(a => a.EmployeeId == employeeId)
                .GroupJoin(_context.AttendanceCheckOuts,
                    ci => new { ci.EmployeeId, ci.AttendanceDate, ci.Shift },
                    co => new { co.EmployeeId, co.AttendanceDate, co.Shift },
                    (ci, coGroup) => new { CheckIn = ci, CheckOuts = coGroup })
                .SelectMany(x => x.CheckOuts.DefaultIfEmpty(), (ci, co) => new
                {
                    ci.CheckIn.AttendanceDate,
                    ci.CheckIn.Shift,
                    ci.CheckIn.CheckInTime,
                    CheckOutTime = co != null ? co.CheckOutTime : (DateTime?)null,
                    OnTimeStatus = ci.CheckIn.Shift == "Ca sáng"
                        ? (ci.CheckIn.CheckInTime.TimeOfDay <= morningShiftStart + gracePeriod ? "On Time" : "Late")
                        : (ci.CheckIn.CheckInTime.TimeOfDay <= afternoonShiftStart + gracePeriod ? "On Time" : "Late")
                })
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Shift)
                .ToListAsync();

            if (attendanceRecords == null || attendanceRecords.Count == 0)
            {
                return NotFound("No attendance records found.");
            }

            return Ok(attendanceRecords);
        }

        [HttpGet("AttendanceReport/Range")]
        public async Task<IActionResult> GetAttendanceReportByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Chuẩn hóa startDate và endDate để chỉ lấy phần ngày
            startDate = startDate.Date;
            endDate = endDate.Date;

            if (startDate > endDate)
                return BadRequest("startDate must be earlier than endDate.");

            var allEmployees = await _context.Employees.ToListAsync();
            var checkIns = await _context.AttendanceCheckIns
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                .Include(a => a.Employee)
                .ToListAsync();
            var checkOuts = await _context.AttendanceCheckOuts
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                .ToListAsync();

            var dateRangeReport = new Dictionary<DateTime, object>();

            for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
            {
                var attendedRecords = checkIns
                    .Where(a => a.AttendanceDate.Date == currentDate)
                    .Select(a => new
                    {
                        a.Employee.Id,
                        a.Employee.FullName,
                        a.AttendanceDate,
                        a.Employee.Image,
                        a.Employee.BirthDate,
                        a.Shift,
                        a.CheckInTime,
                        CheckOutTime = checkOuts
                            .FirstOrDefault(co => co.EmployeeId == a.EmployeeId
                                && co.AttendanceDate.Date == currentDate
                                && co.Shift == a.Shift)?.CheckOutTime,
                        Status = "Attended"
                    })
                    .ToList();

                var attendedEmployeeShifts = attendedRecords
                    .Select(a => $"{a.Id}-{a.Shift}")
                    .ToHashSet();

                var notAttendedEmployees = allEmployees
                    .SelectMany(e => new[] { "Ca sáng", "Ca chiều" }
                        .Where(shift => !attendedEmployeeShifts.Contains($"{e.Id}-{shift}"))
                        .Select(shift => new
                        {
                            e.Id,
                            e.FullName,
                            e.BirthDate,
                            e.Image,
                            Shift = shift,
                            CheckInTime = (DateTime?)null,
                            CheckOutTime = (DateTime?)null,
                            Status = "Not Attended"
                        }))
                    .ToList();

                dateRangeReport[currentDate] = new
                {
                    Date = currentDate,
                    AttendedRecords = attendedRecords,
                    NotAttendedRecords = notAttendedEmployees
                };
            }

            return Ok(dateRangeReport);
        }
        [HttpGet("GetEmployees")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.Employees
                .Select(e => new
                {
                    e.Id,
                    e.FullName,
                    e.BirthDate,
                    e.Gender,
                    e.PhoneNumber,
                    e.IdentityNumber,
                    e.StartDate
                })
                .ToListAsync();

            return Ok(employees);
        }

        [HttpGet("CheckAttendanceStatus")]
        public async Task<IActionResult> CheckAttendanceStatus([FromQuery] int employeeId, [FromQuery] string shift)
        {
            if (employeeId <= 0)
            {
                return BadRequest("Invalid Employee ID.");
            }

            if (shift != "Ca sáng" && shift != "Ca chiều")
            {
                return BadRequest("Shift must be 'Ca sáng' or 'Ca chiều'.");
            }

            var today = DateTime.Today;
            var morningShiftStart = new TimeSpan(8, 0, 0);
            var afternoonShiftStart = new TimeSpan(15, 0, 0);
            var gracePeriod = new TimeSpan(0, 15, 0);

            var checkIn = await _context.AttendanceCheckIns
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                    && a.AttendanceDate == today
                    && a.Shift == shift);

            if (checkIn == null)
            {
                return Ok(new
                {
                    Status = "Not Checked In",
                    Shift = shift,
                    Message = $"Employee has not checked in for {shift} today."
                });
            }

            var checkOut = await _context.AttendanceCheckOuts
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                    && a.AttendanceDate == today
                    && a.Shift == shift);

            var onTimeStatus = checkIn.Shift == "Ca sáng"
                ? (checkIn.CheckInTime.TimeOfDay <= morningShiftStart + gracePeriod ? "On Time" : "Late")
                : (checkIn.CheckInTime.TimeOfDay <= afternoonShiftStart + gracePeriod ? "On Time" : "Late");

            return Ok(new
            {
                Status = checkOut != null ? "Checked Out" : "Checked In",
                AttendanceDate = checkIn.AttendanceDate,
                Shift = checkIn.Shift,
                CheckInTime = checkIn.CheckInTime,
                CheckOutTime = checkOut?.CheckOutTime,
                OnTimeStatus = onTimeStatus
            });
        }

        public class CheckInRequest
        {
            public int EmployeeId { get; set; }
        }

        public class CheckOutRequest
        {
            public int EmployeeId { get; set; }
        }
    }
}
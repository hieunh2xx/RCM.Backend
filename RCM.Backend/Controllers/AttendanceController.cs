using DataLayerObject.Models;
using Microsoft.AspNetCore.Http;
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
            var lateTime = new TimeSpan(8, 0, 0); // 8:00 sáng

            // Đọc số tiền phạt từ appsettings.json
            int penaltyAmount = _configuration.GetValue<int>("PenaltySettings:LateCheckInPenalty");

            // Kiểm tra xem nhân viên đã check-in hôm nay chưa
            var existingAttendance = await _context.AttendanceHis
                .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && a.AttendanceDate == now.Date);

            if (existingAttendance != null)
            {
                return BadRequest("Nhân viên đã check-in hôm nay.");
            }

            bool isOnTime = currentTime <= lateTime;
            var attendance = new AttendanceHi
            {
                EmployeeId = request.EmployeeId,
                AttendanceDate = now.Date,
                Shift = "Ca sáng",
                CheckInTime = now,
                OnTime = isOnTime ? 1 : 0
            };

            _context.AttendanceHis.Add(attendance);
            await _context.SaveChangesAsync();

            // Nếu nhân viên đi muộn, ghi nhận khoản phạt
            if (!isOnTime)
            {
                var penalty = new PenaltyPayment
                {
                    EmployeeId = request.EmployeeId,
                    Amount = penaltyAmount,
                    PaymentDate = now.Date,
                    Reason = "Đi làm muộn",
                    PaymentMethod = 0, // 0 = Chưa thanh toán, nhân viên tự nộp phạt
                    Note = "Chưa thanh toán tiền phạt đi làm muộn",
                    IsDeleted = false
                };
                var salary = await _context.Salaries.FirstOrDefaultAsync(s => s.EmployeeId == request.EmployeeId && s.EndDate == null);
                if (salary != null)
                {
                    salary.BonusSalary -= penaltyAmount;
                    _context.Salaries.Update(salary);
                }
                _context.PenaltyPayments.Add(penalty);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Thông_báo = "Check-in thành công.",
                Trạng_thái = isOnTime ? "Đúng giờ" : "Đi muộn",
                Thời_gian_checkin = now.ToString("dd/MM/yyyy HH:mm:ss"),
                Tiền_phạt = isOnTime ? "Không có" : $"Bị phạt {penaltyAmount:N0} VNĐ (Chưa thanh toán)"
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
            var checkOutDeadline = new TimeSpan(17, 0, 0); // 5:00 PM

            if (currentTime < checkOutDeadline)
            {
                return BadRequest("Check-out is not allowed before 17:00.");
            }

            var attendance = await _context.AttendanceHis
                .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && a.AttendanceDate == now.Date);

            if (attendance == null)
            {
                return BadRequest("No check-in record found for today.");
            }

            if (attendance.CheckOutTime != null)
            {
                return BadRequest("Employee has already checked out today.");
            }

            attendance.CheckOutTime = now;
            _context.AttendanceHis.Update(attendance);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Check-out successful.",
                CheckOutTime = now.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }

        [HttpGet("AttendanceDetail")]
        public async Task<IActionResult> GetAttendance([FromQuery] int employeeId)
        {
            if (employeeId <= 0)
            {
                return BadRequest("Invalid Employee ID.");
            }

            var attendanceRecords = await _context.AttendanceHis
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AttendanceDate)
                .Select(a => new
                {
                    a.AttendanceDate,
                    a.Shift,
                    a.CheckInTime,
                    a.CheckOutTime,
                    OnTimeStatus = a.OnTime == 1 ? "On Time" : "Late"
                })
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
            if (startDate > endDate)
                return BadRequest("startDate must be earlier than endDate.");

            // Lấy danh sách tất cả nhân viên
            var allEmployees = await _context.Employees.ToListAsync();

            // Lấy dữ liệu điểm danh trong khoảng thời gian được truyền vào
            var attendanceRecords = await _context.AttendanceHis
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                .Include(a => a.Employee) // Load thông tin nhân viên
                .ToListAsync();

            // Tạo dictionary để nhóm dữ liệu theo ngày
            var dateRangeReport = new Dictionary<DateTime, object>();

            for (DateTime currentDate = startDate.Date; currentDate <= endDate.Date; currentDate = currentDate.AddDays(1))
            {
                // Lọc danh sách nhân viên đã điểm danh trong ngày hiện tại
                var attendedEmployees = attendanceRecords
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
                        a.CheckOutTime,
                        Status = "Attended"
                    })
                    .ToList();

                // Lấy danh sách ID nhân viên đã điểm danh
                var attendedEmployeeIds = attendedEmployees.Select(a => a.Id).ToHashSet();

                // Lọc danh sách nhân viên chưa điểm danh trong ngày
                var notAttendedEmployees = allEmployees
                    .Where(e => !attendedEmployeeIds.Contains(e.Id))
                    .Select(e => new
                    {
                        e.Id,
                        e.FullName,
                        e.BirthDate,
                        e.Image,
                        Shift = "N/A",
                        CheckInTime = (DateTime?)null,
                        CheckOutTime = (DateTime?)null,
                        Status = "Not Attended"
                    })
                    .ToList();

                // Thêm vào báo cáo
                dateRangeReport[currentDate] = new
                {
                    Date = currentDate,
                    AttendedEmployees = attendedEmployees,
                    NotAttendedEmployees = notAttendedEmployees
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
                    e.StartDate,
                    e.ActiveStatus
                })
                .ToListAsync();

            return Ok(employees);
        }

        //[HttpPost("AddEmployee")]
        //public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        //{
        //    if (employee == null)
        //        return BadRequest("Invalid data.");

        //    _context.Employees.Add(employee);
        //    await _context.SaveChangesAsync();
        //    return Ok("Employee added successfully.");
        //}

        public class CheckInRequest
        {
            public int EmployeeId { get; set; }
        }

        public class CheckOutRequest
        {
            public int EmployeeId { get; set; }
        }
        [HttpGet("CheckAttendanceStatus")]
        public async Task<IActionResult> CheckAttendanceStatus([FromQuery] int employeeId)
        {
            if (employeeId <= 0)
            {
                return BadRequest("Invalid Employee ID.");
            }

            var today = DateTime.Today;

            var attendance = await _context.AttendanceHis
                .Where(a => a.EmployeeId == employeeId && a.AttendanceDate == today)
                .FirstOrDefaultAsync();

            if (attendance == null)
            {
                return Ok(new
                {
                    Status = "Not Checked In",
                    Message = "Employee has not checked in today."
                });
            }

            return Ok(new
            {
                Status = attendance.CheckOutTime != null ? "Checked Out" : "Checked In",
                AttendanceDate = attendance.AttendanceDate,
                Shift = attendance.Shift,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                OnTimeStatus = attendance.OnTime == 1 ? "On Time" : "Late"
            });
        }

    }
}

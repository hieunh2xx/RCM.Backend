import React, { useEffect, useState } from "react";
import { format, isPast, isFuture } from "date-fns";

const employeeId = 2; // ID nhân viên giả định

const Calendar = () => {
  const [attendance, setAttendance] = useState({});
  const [selectedDate, setSelectedDate] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [currentMonth, setCurrentMonth] = useState(new Date().getMonth());

  useEffect(() => {
    fetch(
      `http://localhost:5000/api/Attendance/AttendanceDetail?employeeId=${employeeId}`
    )
      .then((res) => res.json())
      .then((data) => {
        const formattedAttendance = data.reduce((acc, entry) => {
          const dateStr = format(new Date(entry.attendanceDate), "yyyy-MM-dd");

          acc[dateStr] = {
            checkIn: entry.checkInTime
              ? format(new Date(entry.checkInTime), "HH:mm:ss")
              : null,
            checkOut: entry.checkOutTime
              ? format(new Date(entry.checkOutTime), "HH:mm:ss")
              : null,
          };

          return acc;
        }, {});

        setAttendance(formattedAttendance);
      })
      .catch((error) => console.error("Lỗi lấy dữ liệu:", error));
  }, []);

  // Xác định màu sắc của ô ngày dựa trên trạng thái chấm công
  const getColorClass = (dateStr) => {
    const todayStr = format(new Date(), "yyyy-MM-dd");
    const entry = attendance[dateStr];

    if (!entry) {
      return dateStr < todayStr ? "bg-red-300 text-white" : "bg-gray-200"; // Ngày cũ chưa check-in -> đỏ
    }

    if (entry.checkIn && !entry.checkOut) {
      return "bg-orange-500 text-white"; // Đã Check-in nhưng chưa Check-out
    }

    if (entry.checkIn && entry.checkOut) {
      return "bg-green-500 text-white"; // Đã Check-in và Check-out
    }

    return "bg-gray-200"; // Ngày chưa đến hoặc không có dữ liệu
  };

  // Mở modal khi click vào ngày
  const openModal = (date) => {
    const dateStr = format(date, "yyyy-MM-dd");
    setSelectedDate(dateStr);
    setModalOpen(true);
  };

  // Chuyển tháng trước hoặc sau
  const changeMonth = (direction) => {
    setCurrentMonth((prev) => prev + direction);
  };

  // Tạo lịch
  const firstDay = new Date(new Date().getFullYear(), currentMonth, 1);
  const daysInMonth = new Date(
    new Date().getFullYear(),
    currentMonth + 1,
    0
  ).getDate();
  const paddedDates = Array(firstDay.getDay())
    .fill(null)
    .concat(
      Array.from(
        { length: daysInMonth },
        (_, i) => new Date(new Date().getFullYear(), currentMonth, i + 1)
      )
    );

  return (
    <div className="p-6">
      {/* Tiêu đề tháng */}
      <div className="flex justify-between items-center mb-4">
        <button
          className="p-2 bg-gray-300 rounded"
          onClick={() => changeMonth(-1)}
        >
          ◀
        </button>
        <h2 className="text-xl font-bold">{format(firstDay, "MMMM yyyy")}</h2>
        <button
          className="p-2 bg-gray-300 rounded"
          onClick={() => changeMonth(1)}
        >
          ▶
        </button>
      </div>

      {/* Lưới lịch */}
      <div className="grid grid-cols-7 gap-2">
        {["CN", "T2", "T3", "T4", "T5", "T6", "T7"].map((day) => (
          <div key={day} className="font-semibold text-center">
            {day}
          </div>
        ))}

        {paddedDates.map((date, index) => {
          if (!date) return <div key={index} className="h-24"></div>;

          const dateStr = format(date, "yyyy-MM-dd");
          const bgColor = getColorClass(dateStr);

          return (
            <div
              key={index}
              className={`h-24 p-2 border rounded cursor-pointer flex flex-col justify-center items-center ${bgColor}`}
              onClick={() => openModal(date)}
            >
              <div className="font-semibold">
                {format(date, "d")}/{currentMonth + 1}
              </div>
            </div>
          );
        })}
      </div>

      {/* Modal */}
      {modalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center">
          <div className="bg-white p-6 rounded shadow-lg">
            <h2 className="text-xl font-bold mb-2">
              Chi tiết ngày {selectedDate}
            </h2>

            {attendance[selectedDate] ? (
              <div>
                <p>
                  <strong>Check-in:</strong>{" "}
                  {attendance[selectedDate].checkIn || "Chưa Check-in"}
                </p>
                <p>
                  <strong>Check-out:</strong>{" "}
                  {attendance[selectedDate].checkIn
                    ? attendance[selectedDate].checkOut || "Chưa Check-out"
                    : "-"}
                </p>

                {/* Nút Check-in nếu chưa Check-in và ngày hiện tại */}
                {selectedDate === format(new Date(), "yyyy-MM-dd") &&
                  !attendance[selectedDate].checkIn && (
                    <button className="mt-4 p-2 bg-blue-500 text-white rounded">
                      Check-in
                    </button>
                  )}

                {/* Nút Check-out nếu đã Check-in */}
                {attendance[selectedDate].checkIn &&
                  !attendance[selectedDate].checkOut && (
                    <button className="mt-4 p-2 bg-red-500 text-white rounded">
                      Check-out
                    </button>
                  )}
              </div>
            ) : (
              <p>Không có dữ liệu chấm công.</p>
            )}

            <button
              className="mt-4 p-2 bg-gray-400 text-white rounded"
              onClick={() => setModalOpen(false)}
            >
              Đóng
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default Calendar;

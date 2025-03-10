import React, { useEffect, useState } from "react";
import { format, parseISO } from "date-fns";
import { vi } from "date-fns/locale";

const employeeId = 2;

const Calendar = () => {
  const [attendance, setAttendance] = useState({});
  const [selectedDate, setSelectedDate] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [currentMonth, setCurrentMonth] = useState(new Date().getMonth());

  useEffect(() => {
    fetchAttendanceData();
  }, []);

  const fetchAttendanceData = () => {
    fetch(
      `http://localhost:5000/api/Attendance/AttendanceDetail?employeeId=${employeeId}`
    )
      .then((res) => res.json())
      .then((data) => {
        console.log("Dữ liệu từ API:", data);

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

        console.log("Dữ liệu chấm công đã format:", formattedAttendance);
        setAttendance(formattedAttendance);
      })
      .catch((error) => console.error("Lỗi lấy dữ liệu:", error));
  };

  const handleCheckIn = () => {
    fetch("http://localhost:5000/api/Attendance/CheckIn", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ employeeId }),
    })
      .then((res) => res.json())
      .then((data) => {
        console.log("Check-in thành công:", data);
        fetchAttendanceData(); // Cập nhật lại dữ liệu sau khi check-in
        setModalOpen(false);
      })
      .catch((error) => console.error("Lỗi check-in:", error));
  };

  const handleCheckOut = () => {
    fetch("http://localhost:5000/api/Attendance/CheckOut", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ employeeId }),
    })
      .then((res) => res.json())
      .then((data) => {
        console.log("Check-out thành công:", data);
        fetchAttendanceData(); // Cập nhật lại dữ liệu sau khi check-out
        setModalOpen(false);
      })
      .catch((error) => console.error("Lỗi check-out:", error));
  };
  const getColorClass = (dateStr) => {
    const todayStr = format(new Date(), "yyyy-MM-dd");
    const entry = attendance[dateStr];

    if (!entry) {
      return dateStr < todayStr ? "bg-red-300 text-white" : "bg-gray-200";
    }
    if (entry.checkIn && !entry.checkOut) {
      return "bg-yellow-500 text-white";
    }
    if (entry.checkIn && entry.checkOut) {
      return "bg-green-500 text-white";
    }
    return "bg-gray-200";
  };

  const openModal = (date) => {
    const dateStr = format(date, "yyyy-MM-dd");
    console.log("Ngày được chọn:", dateStr);
    setSelectedDate(dateStr);
    setModalOpen(true);
  };

  const changeMonth = (direction) => {
    setCurrentMonth((prev) => prev + direction);
  };

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
      <div className="flex justify-between items-center mb-4">
        <button
          className="p-2 bg-gray-300 rounded"
          onClick={() => changeMonth(-1)}
        >
          ◀
        </button>
        <h2 className="text-xl font-bold uppercase">
          {" "}
          {format(firstDay, "MMMM yyyy", { locale: vi })}
        </h2>
        <button
          className="p-2 bg-gray-300 rounded"
          onClick={() => changeMonth(1)}
        >
          ▶
        </button>
      </div>

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

      {modalOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center">
          <div className="bg-white p-6 rounded shadow-lg">
            <h2 className="text-xl font-bold mb-2">
              Chi tiết ngày {selectedDate}
            </h2>
            {(() => {
              const now = new Date();
              const currentHour = now.getHours();
              const todayStr = format(now, "yyyy-MM-dd");
              const isToday = selectedDate === todayStr;
              const hasCheckedIn = attendance[selectedDate]?.checkIn;
              const hasCheckedOut = attendance[selectedDate]?.checkOut;

              console.log("Giờ hiện tại:", currentHour);
              console.log("Ngày hiện tại:", todayStr);
              console.log("Trạng thái chấm công:", attendance[selectedDate]);

              return (
                <>
                  <p>
                    <strong>Check-in:</strong> {hasCheckedIn || "Chưa Check-in"}
                  </p>
                  <p>
                    <strong>Check-out:</strong>{" "}
                    {hasCheckedIn ? hasCheckedOut || "Chưa Check-out" : "-"}
                  </p>

                  {isToday && !hasCheckedIn && currentHour < 17 && (
                    <button
                      className="mt-4 p-2 bg-blue-500 text-white rounded mr-16"
                      onClick={handleCheckIn}
                    >
                      Check-in
                    </button>
                  )}
                  {hasCheckedIn && !hasCheckedOut && currentHour >= 17 && (
                    <button
                      className="mt-4 p-2 bg-red-500 text-white rounded mr-16"
                      onClick={handleCheckOut}
                    >
                      Check-out
                    </button>
                  )}
                </>
              );
            })()}
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

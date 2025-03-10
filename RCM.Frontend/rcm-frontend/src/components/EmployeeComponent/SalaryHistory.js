import React, { useState, useEffect } from "react";
import Header from "../../headerComponent/header";
import { toast } from "react-toastify";

const SalaryHistory = () => {
  const [data, setData] = useState([]);
  const currentDate = new Date();
  const currentYear = currentDate.getFullYear();
  const currentMonth = currentDate.getMonth() + 1;
  const [selectedYear, setSelectedYear] = useState(currentYear);
  const [selectedMonth, setSelectedMonth] = useState(currentMonth);

  useEffect(() => {
    fetch(
      `http://localhost:5000/api/Payroll/list?month=${selectedMonth}&year=${selectedYear}`
    )
      .then((res) => res.json())
      .then((data) => setData(data))
      .catch((err) => console.error("Error fetching data:", err));
  }, [selectedMonth, selectedYear]);

  const exportFile = async () => {
    try {
      const response = await fetch(
        `http://localhost:5000/api/Payroll/export?month=${selectedMonth}&year=${selectedYear}`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error("Lỗi khi tải file từ server");
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `LichSuLuong_${selectedMonth}_${selectedYear}.xlsx`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
      toast.success("Tải file thành công!");
    } catch (error) {
      console.error("Lỗi khi tải file:", error);
      toast.error("Tải file thất bại. Vui lòng thử lại!");
    }
  };

  return (
    <div>
      <Header />
      <div className="p-10">
        <div className="p-4 border-b bg-white shadow">
          <div className="flex justify-between items-center my-3">
            <div className="text-lg font-bold">Lịch Sử Lương Nhân Viên</div>
            <div className="flex gap-4 items-center">
              <select
                value={selectedYear}
                onChange={(e) => setSelectedYear(parseInt(e.target.value))}
                className="border border-gray-300 p-2 rounded w-32 text-center shadow-sm"
              >
                {[...Array(5)].map((_, idx) => (
                  <option key={idx} value={currentYear - 2 + idx}>
                    {currentYear - 2 + idx}
                  </option>
                ))}
              </select>

              <select
                value={selectedMonth}
                onChange={(e) => setSelectedMonth(parseInt(e.target.value))}
                className="border border-gray-300 p-2 rounded w-32 text-center shadow-sm"
              >
                {[...Array(12)].map((_, idx) => (
                  <option key={idx} value={idx + 1}>
                    Tháng {idx + 1}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <div className="flex justify-between items-center mb-4">
            <div className="d-flex gap-2 w-50">
              <input
                type="text"
                className="form-control"
                placeholder="Tìm kiếm"
                style={{ width: "30rem" }}
              />
              <button className="btn btn-primary d-flex align-items-center px-4">
                Tìm kiếm
              </button>
            </div>
            <div className="space-x-2">
              <button
                className="bg-green-500 text-white px-4 py-2 rounded ml-2"
                onClick={exportFile}
              >
                Xuất Excel
              </button>
            </div>
          </div>
        </div>
        <div className="overflow-x-auto mt-4">
          <table className="w-full border-collapse border">
            <thead>
              <tr className="bg-gray-100">
                <th className="border p-2 text-center">ID Nhân viên</th>
                <th className="border p-2 text-center">Tên nhân viên</th>
                <th className="border p-2 text-center">SĐT</th>
                <th className="border p-2 text-center">Lương cố định</th>
                <th className="border p-2 text-center">Số ngày công</th>
                <th className="border p-2 text-center">Lương ngày</th>
                <th className="border p-2 text-center">Thưởng</th>
                <th className="border p-2 text-center">Phạt</th>
                <th className="border p-2 text-center">Tổng lương</th>
              </tr>
            </thead>
            <tbody>
              {data.map((item) => (
                <tr key={item.employeeId} className="hover:bg-gray-50">
                  <td className="border p-2 text-center">{item.employeeId}</td>
                  <td className="border p-2">{item.employeeName}</td>
                  <td className="border p-2 text-center">{item.phone}</td>
                  <td className="border p-2 text-center">
                    {new Intl.NumberFormat("vi-VN", {
                      style: "currency",
                      currency: "VND",
                    }).format(item.fixedSalary)}
                  </td>
                  <td className="border p-2 text-center">
                    {item.totalWorkDays}
                  </td>
                  <td className="border p-2 text-center">
                    {new Intl.NumberFormat("vi-VN", {
                      style: "currency",
                      currency: "VND",
                    }).format(item.dailySalary)}
                  </td>
                  <td className="border p-2 text-center">
                    {new Intl.NumberFormat("vi-VN", {
                      style: "currency",
                      currency: "VND",
                    }).format(item.bonusSalary)}
                  </td>
                  <td className="border p-2 text-center">{item.penalty}</td>
                  <td className="border p-2 text-center font-bold">
                    {new Intl.NumberFormat("vi-VN", {
                      style: "currency",
                      currency: "VND",
                    }).format(item.totalSalary)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default SalaryHistory;

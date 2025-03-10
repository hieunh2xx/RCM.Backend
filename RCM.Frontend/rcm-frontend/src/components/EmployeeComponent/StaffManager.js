import React, { useEffect, useState } from "react";
import axios from "axios";
import { useForm } from "react-hook-form";
import { toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import Header from "../../headerComponent/header";
export default function StaffManager() {
  const [staffList, setStaffList] = useState([]);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [warehouses, setWarehouses] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedStaff, setSelectedStaff] = useState(null); // Nhân viên đang sửa
  const { register, handleSubmit, reset, setValue } = useForm({
    defaultValues: {
      role: 2, // Mặc định role = 2
      startDate: new Date().toISOString().split("T")[0],
    },
  });
  useEffect(() => {
    const delayDebounceFn = setTimeout(() => {
      fetchStaff(searchTerm);
    }, 300); // Chờ 300ms để tránh gọi API quá nhiều lần

    return () => clearTimeout(delayDebounceFn);
  }, [searchTerm]);
  const fetchWarehouses = async () => {
    try {
      const response = await axios.get(
        "http://localhost:5000/api/Warehouses/GetWarehouses"
      );
      setWarehouses(response.data);
    } catch (error) {
      console.error("Lỗi khi lấy danh sách kho hàng:", error);
    }
  };
  const fetchStaff = async (search = "") => {
    try {
      const response = await axios.get(
        `http://localhost:5000/api/Staff/getStaff?name=${search}`
      );
      setStaffList(response.data);
    } catch (error) {
      console.error("Lỗi khi lấy danh sách nhân viên:", error);
    }
  };

  const handleSearch = () => {
    fetchStaff(searchTerm);
  };
  const openUpdateModal = async (id) => {
    try {
      const response = await axios.get(`http://localhost:5000/api/Staff/${id}`);
      const staffData = response.data;
      setSelectedStaff(staffData);

      setValue("fullName", staffData.fullName);
      setValue("username", staffData.username);
      setValue(
        "birthDate",
        new Date(staffData.birthDate).toISOString().split("T")[0]
      );
      setValue("gender", staffData.gender);
      setValue("phoneNumber", staffData.phoneNumber);
      setValue("identityNumber", staffData.identityNumber);
      setValue("hometown", staffData.hometown);
      setValue("currentAddress", staffData.currentAddress);
      setValue("branchId", staffData.branchId);
      setIsModalOpen(true);
    } catch (error) {
      console.error("Lỗi khi lấy thông tin nhân viên:", error);
      toast.error("Không thể lấy thông tin nhân viên!", {
        position: "top-right",
      });
    }
  };
  const onSubmit = async (data) => {
    try {
      if (selectedStaff) {
        await axios.put(
          `http://localhost:5000/api/Staff/update-employee/${selectedStaff.id}`,
          {
            ...data,
            fixedSalary: Number(data.fixedSalary),
            birthDate: new Date(data.birthDate).toISOString(),
          }
        );
        toast.success("Cập nhật nhân viên thành công!", {
          position: "top-right",
        });
      } else {
        await axios.post("http://localhost:5000/api/Staff/add-employee", {
          ...data,
          id: 0,
          role: 2, // Luôn đặt role là 2
          workShiftId: Number(data.workShiftId),
          branchId: Number(data.branchId),
          fixedSalary: Number(data.fixedSalary),
          birthDate: new Date(data.birthDate).toISOString(),
          startDate: new Date().toISOString().split("T")[0],
        });
        toast.success("Thêm nhân viên thành công!", { position: "top-right" });
      }

      closeModal();
      fetchStaff();
    } catch (error) {
      const errorMessage = error.response?.data?.message || "Đã có lỗi xảy ra!";
      toast.error(errorMessage, { position: "top-right" });
    }
  };
  const closeModal = () => {
    setIsModalOpen(false);
    reset();
  };
  // Xử lý chọn file
  const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (!file) return;

    const validExtensions = ["xlsx", "xls", "csv"];
    const fileExtension = file.name.split(".").pop().toLowerCase();

    if (!validExtensions.includes(fileExtension)) {
      toast.error("Chỉ hỗ trợ file Excel (.xlsx, .xls) hoặc CSV!", {
        position: "top-right",
      });
      return;
    }

    uploadFile(file);
  };
  // Gửi file lên API
  const uploadFile = async (file) => {
    const formData = new FormData();
    formData.append("file", file);

    try {
      await axios.post("http://localhost:5000/api/Staff/import", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      toast.success("Nhập file thành công!", { position: "top-right" });
      fetchStaff(); // Load lại danh sách nhân viên
    } catch (error) {
      toast.error("Lỗi khi nhập file!", { position: "top-right" });
    }
  };
  // Xử lý xuất file
  const exportFile = async (format) => {
    try {
      const response = await axios.get(
        `http://localhost:5000/api/Staff/export?format=${format}`,
        {
          responseType: "blob", // Để xử lý file
        }
      );

      // Tạo link tải file
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `DanhSachNhanVien.${format}`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch (error) {
      toast.error("Lỗi khi xuất file!", { position: "top-right" });
    }
  };
  return (
    <>
      <Header />
      <div className="p-10 h-screen bg-gray-100">
        <div className="flex justify-between items-center mb-4">
          <div className="d-flex gap-2 w-50">
            <input
              type="text"
              className="form-control"
              placeholder="Theo tên nhân viên"
              style={{ width: "30rem" }}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
            <button
              className="btn btn-primary d-flex align-items-center px-4"
              onClick={handleSearch}
            >
              Tìm kiếm
            </button>
          </div>
          <div className="space-x-2">
            <button
              className="bg-green-500 text-white px-4 py-2 rounded"
              onClick={() => setIsModalOpen(true)}
            >
              Thêm nhân viên
            </button>
            <input
              type="file"
              accept=".xlsx, .xls, .csv"
              className="hidden"
              id="fileInput"
              onChange={handleFileChange}
            />
            <button
              className="bg-blue-500 text-white px-4 py-2 rounded"
              onClick={() => document.getElementById("fileInput").click()}
            >
              Nhập File
            </button>
            <button
              className="bg-green-500 text-white px-4 py-2 rounded ml-2"
              onClick={() => exportFile("xlsx")}
            >
              Xuất Excel
            </button>
            <button
              className="bg-yellow-500 text-white px-4 py-2 rounded ml-2"
              onClick={() => exportFile("csv")}
            >
              Xuất CSV
            </button>
          </div>
        </div>
        <table className="w-full bg-white shadow-md rounded">
          <thead className="bg-gray-100">
            <tr>
              <th className="p-2 text-center">Mã nhân viên</th>
              <th className="p-2 text-center">Ảnh hồ sơ</th>
              <th className="p-2 text-center">Họ tên nhân viên</th>
              <th className="p-2 text-center">Ngày sinh</th>
              <th className="p-2 text-center">Giới tính</th>
              <th className="p-2 text-center">Số điện thoại</th>
              <th className="p-2 text-center">Ngày vào làm</th>
              <th className="p-2">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {staffList.length > 0 ? (
              staffList.map((staff) => (
                <tr key={staff.id}>
                  <td className="p-2 text-center">{staff.id}</td>
                  <td className="p-2 flex justify-center">
                    <img
                      src={
                        staff.profileImage ||
                        "https://icons.veryicon.com/png/o/miscellaneous/standard/avatar-15.png"
                      }
                      alt="Ảnh hồ sơ"
                      width="50"
                      height="50"
                    />
                  </td>
                  <td className="p-2 text-center">{staff.fullName}</td>
                  <td className="p-2 text-center">
                    {new Date(staff.birthDate).toLocaleDateString("vi-VN")}
                  </td>
                  <td className="p-2 text-center">
                    {staff.gender === "Female" ? "Nữ" : "Nam"}
                  </td>
                  <td className="p-2 text-center">{staff.phoneNumber}</td>
                  <td className="p-2 text-center">
                    {new Date(staff.startDate).toLocaleDateString("vi-VN")}
                  </td>
                  <td className="p-2 space-x-2">
                    <button
                      className="bg-green-500 text-white px-2 py-1 rounded"
                      onClick={() => openUpdateModal(staff.id)}
                    >
                      Sửa
                    </button>
                    <button className="bg-red-500 text-white px-2 py-1 rounded">
                      Xóa
                    </button>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="7" className="text-center p-4">
                  Không có dữ liệu
                </td>
              </tr>
            )}
          </tbody>
        </table>
        {/* Modal thêm nhân viên */}
        {isModalOpen && (
          <div
            className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center"
            onClick={closeModal}
          >
            <div
              className="bg-white p-6 rounded-lg w-1/3 max-h-screen overflow-y-auto"
              onClick={(e) => e.stopPropagation()}
            >
              <h2 className="text-lg font-bold mb-4">
                {" "}
                {selectedStaff ? "Cập nhật nhân viên" : "Thêm nhân viên"}
              </h2>
              <form onSubmit={handleSubmit(onSubmit)}>
                <div className="space-y-2">
                  <label className="block font-medium">Họ tên</label>
                  <input
                    {...register("fullName")}
                    className="w-full p-2 border rounded"
                    required
                  />
                  <label className="block font-medium">Tên đăng nhập</label>
                  <input
                    {...register("username")}
                    className="w-full p-2 border rounded"
                    required
                  />
                  <label className="block font-medium">Ngày sinh</label>
                  <input
                    type="date"
                    {...register("birthDate")}
                    className="w-full p-2 border rounded"
                    required
                  />
                  <label className="block font-medium">Giới tính</label>
                  <select
                    {...register("gender")}
                    className="w-full p-2 border rounded"
                  >
                    <option value="Male">Nam</option>
                    <option value="Female">Nữ</option>
                  </select>
                  <label className="block font-medium">Số điện thoại</label>
                  <input
                    {...register("phoneNumber")}
                    className="w-full p-2 border rounded"
                    required
                  />
                  <label className="block font-medium">Ca làm việc</label>
                  <select
                    {...register("workShiftId")}
                    className="w-full p-2 border rounded"
                  >
                    <option value="1">Ca sáng</option>
                    <option value="2">Ca chiều</option>
                  </select>
                  <label className="block font-medium">Kho Hàng</label>
                  <select
                    {...register("branchId")}
                    className="w-full p-2 border rounded"
                  >
                    <option value="">Chọn Kho Hàng</option>
                    {warehouses.map((warehouse) => (
                      <option key={warehouse.id} value={warehouse.id}>
                        {warehouse.name}
                      </option>
                    ))}
                  </select>
                  <label className="block font-medium">Lương cố định</label>
                  <input
                    type="number"
                    {...register("fixedSalary")}
                    className="w-full p-2 border rounded"
                    required
                  />
                  <label className="block font-medium">Số CMND/CCCD</label>
                  <input
                    type="text"
                    {...register("identityNumber")}
                    className="w-full p-2 border rounded"
                    required
                  />
                  <label className="block font-medium">Quê quán</label>
                  <input
                    type="text"
                    {...register("hometown")}
                    className="w-full p-2 border rounded"
                    required
                  />
                  <label className="block font-medium">Địa chỉ hiện tại</label>
                  <input
                    type="text"
                    {...register("currentAddress")}
                    className="w-full p-2 border rounded"
                    required
                  />
                </div>
                {/* Nút Lưu và Hủy */}
                <div className="flex justify-end space-x-2 mt-4">
                  <button
                    type="button"
                    className="bg-gray-400 text-white px-4 py-2 rounded"
                    onClick={closeModal}
                  >
                    Hủy
                  </button>
                  <button
                    type="submit"
                    className="bg-blue-500 text-white px-4 py-2 rounded"
                  >
                    {selectedStaff ? "Cập nhật" : "Lưu"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </>
  );
}

import React, { useState } from 'react';
import { useEffect } from "react";
import { Link } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';
import axios from "axios";


const Header = () => {
    const [activeDropdown, setActiveDropdown] = useState(null);
    const [branchDropdown, setBranchDropdown] = useState(false);
    const [accountDropdown, setAccountDropdown] = useState(false);
    const [showProfileModal, setShowProfileModal] = useState(false);

    const navigate = useNavigate();
    const [userInfo, setUserInfo] = useState(null);

    const handleDropdown = (dropdownName) => {
        setActiveDropdown(activeDropdown === dropdownName ? null : dropdownName);
    };

    const toggleBranchDropdown = () => {
        setBranchDropdown((prev) => !prev);
    };

    const toggleAccountDropdown = () => {
        setAccountDropdown((prev) => !prev);
    };

    const closeDropdown = () => {
        setActiveDropdown(null);
        setBranchDropdown(false);
        setAccountDropdown(false);
    };
    useEffect(() => {

        const token = localStorage.getItem("token");
        if (!token) {
            navigate("/login");
        }
    }, [navigate]); // Chạy 1 lần khi Header được render

    const handleLogout = () => {
        // Xóa token khỏi localStorage
        localStorage.removeItem("token");
        localStorage.removeItem("user");

        // Reload lại trang để đảm bảo trạng thái mới
        window.location.href = "/login";
    };
    const fetchUserInfo = async () => {
        try {
            const token = localStorage.getItem("token");

            console.log("🛠 Token được gửi lên API:", token); // 🔥 Kiểm tra token có hợp lệ không

            if (!token) {
                console.error("❌ Không có token trong Local Storage! Người dùng có thể chưa đăng nhập.");
                return;
            }

            const response = await axios.get("http://localhost:5000/api/account/me", {
                headers: { Authorization: `Bearer ${token}` }
            });

            console.log("✅ Dữ liệu từ API:", response.data);
            setUserInfo(response.data);
        } catch (error) {
            console.error("❌ Lỗi khi lấy thông tin người dùng:", error.response ? error.response.data : error);
        }
    };


    return (
        <>
            <header className="bg-white shadow-md">
                <div className="flex items-center justify-between p-4 bg-blue-600">
                    <div className="flex items-center space-x-4">
                        <h1 className="text-2xl font-bold text-white">RCM</h1>
                        <nav className="flex space-x-4">
                            <Link to="/home" className="text-white flex items-center">Tổng quan</Link>
                            <div
                                className="relative"
                                onMouseLeave={closeDropdown}
                            >
                                <button onClick={() => handleDropdown('goods')} className="text-white flex items-center">
                                    Hàng hóa
                                </button>
                                {activeDropdown === 'goods' && (
                                    <div className="absolute bg-white shadow-md rounded p-2 z-50">
                                        <Link to="/button1" className="block px-4 py-2 hover:bg-gray-200">Danh sách sản phẩm</Link>
                                        <Link to="/button2" className="block px-4 py-2 hover:bg-gray-200">Kiểm kho</Link>
                                        <Link to="/button3" className="block px-4 py-2 hover:bg-gray-200">Nhập hàng</Link>
                                    </div>
                                )}
                            </div>
                            <div
                                className="relative"
                                onMouseLeave={closeDropdown}
                            >
                                <button onClick={() => handleDropdown('transactions')} className="text-white flex items-center">
                                    Giao dịch
                                </button>
                                {activeDropdown === 'transactions' && (
                                    <div className="absolute bg-white shadow-md rounded p-2 z-50">
                                        <Link to="/button6" className="block px-4 py-2 hover:bg-gray-200">Bán hàng</Link>
                                        <Link to="/button5" className="block px-4 py-2 hover:bg-gray-200">Đổi trả hàng</Link>
                                        <Link to="/button4" className="block px-4 py-2 hover:bg-gray-200">Phiếu nhập hàng</Link>
                                    </div>
                                )}
                            </div>
                            <div
                                className="relative"
                                onMouseLeave={closeDropdown}
                            >
                                <button onClick={() => handleDropdown('partners')} className="text-white flex items-center">
                                    Đối tác
                                </button>
                                {activeDropdown === 'partners' && (
                                    <div className="absolute bg-white shadow-md rounded p-2 z-50">
                                        <Link to="/button7" className="block px-4 py-2 hover:bg-gray-200">Nhà cung cấp</Link>
                                        <Link to="/button8" className="block px-4 py-2 hover:bg-gray-200">...</Link>
                                        <Link to="/button9" className="block px-4 py-2 hover:bg-gray-200">...</Link>
                                    </div>
                                )}
                            </div>

                            <div
                                className="relative"
                                onMouseLeave={closeDropdown}
                            >
                                <button onClick={() => handleDropdown('employees')} className="text-white flex items-center">
                                    Nhân viên
                                </button>
                                {activeDropdown === 'employees' && (
                                    <div className="absolute bg-white shadow-md rounded p-2 z-50">
                                        <Link to="/button10" className="block px-4 py-2 hover:bg-gray-200">Danh sách nhân viên</Link>
                                        <Link to="/button11" className="block px-4 py-2 hover:bg-gray-200">Lương</Link>
                                        <Link to="/button12" className="block px-4 py-2 hover:bg-gray-200">Thiết lập lương</Link>
                                    </div>
                                )}
                            </div>

                            <Link to="/soquy" className="text-white flex items-center">Sổ quỹ</Link>
                            <Link to="/baocao" className="text-white flex items-center">Báo cáo</Link>
                        </nav>
                    </div>
                    <div className="flex items-center space-x-4">

                        <Link to="/sales" className="bg-white text-blue-600 px-4 py-2 rounded flex items-center">Bán hàng</Link>
                        <div
                            className="relative"
                            onMouseLeave={closeDropdown}
                        >
                            <button onClick={toggleAccountDropdown} className="text-white">
                                <span className="material-icons">Menu</span>
                            </button>
                            {accountDropdown && (
                                <div className="absolute right-0 bg-white shadow-md rounded p-2">
                                    <button
                                        onClick={() => {
                                            fetchUserInfo();
                                            setShowProfileModal(true)
                                        }}
                                        className="block w-full text-left px-4 py-2 hover:bg-gray-200"
                                    >
                                        Thông tin người dùng
                                    </button>

                                    <button
                                        onClick={handleLogout}
                                        className="block w-full text-left px-4 py-2 hover:bg-gray-200"
                                    >
                                        Đăng xuất
                                    </button>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </header>
            {/* Modal Profile */}
            {showProfileModal && (
                <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
                    <div className="bg-white p-6 rounded-lg shadow-lg w-96 relative">
                        <h2 className="text-xl font-bold mb-4">Thông tin người dùng</h2>
                        {userInfo ? (
                            <>
                                <p><strong>Tên đăng nhập:</strong> {userInfo.username}</p>
                                <p><strong>Vai trò:</strong> {userInfo.role}</p>
                                <button
                                    onClick={() => setShowProfileModal(false)}
                                    className="absolute top-2 right-2 text-gray-500 hover:text-red-500"
                                >
                                    ✖
                                </button>
                            </>
                        ) : (
                            <p>Đang tải dữ liệu...</p>
                        )}
                    </div>
                </div>
            )}
        </>
    );
};

export default Header;

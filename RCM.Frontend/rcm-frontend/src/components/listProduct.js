import React from 'react';

const ProductManagementComponent = () => {
    return (
        <div className="flex h-screen bg-gray-100">
            {/* Sidebar */}
            <div className="w-64 bg-white p-6 shadow-md">
                <h2 className="text-xl font-semibold mb-6">Hàng Hóa</h2>
                <ul className="space-y-4">
                    <li><a href="#" className="text-gray-700 hover:bg-gray-200 block p-2 rounded">Quản lý sản phẩm</a></li>
                    <li><a href="#" className="text-gray-700 hover:bg-gray-200 block p-2 rounded">Quản lý kho</a></li>
                    <li><a href="#" className="text-gray-700 hover:bg-gray-200 block p-2 rounded">Đặt hàng nhập</a></li>
                    <li><a href="#" className="text-gray-700 hover:bg-gray-200 block p-2 rounded">Nhập hàng</a></li>
                    <li><a href="#" className="text-gray-700 hover:bg-gray-200 block p-2 rounded">Kiểm hàng</a></li>
                    <li><a href="#" className="text-gray-700 hover:bg-gray-200 block p-2 rounded">Nhà cung cấp</a></li>
                    <li><a href="#" className="text-gray-700 hover:bg-gray-200 block p-2 rounded">Điều chuyển</a></li>
                </ul>
            </div>
            {/* Main Content */}
            <div className="flex-1 p-6">
                <div className="flex justify-between mb-4">
                    <div className="flex space-x-2">
                        <input type="text" placeholder="Theo mã, tên hàng" className="p-2 border rounded" />
                        <button className="bg-blue-500 text-white px-4 py-2 rounded">Tìm kiếm</button>
                    </div>
                    <div className="space-x-2">
                        <button className="bg-green-500 text-white px-4 py-2 rounded">Thêm sản phẩm</button>
                        <button className="bg-blue-500 text-white px-4 py-2 rounded">Nhập File</button>
                        <button className="bg-blue-500 text-white px-4 py-2 rounded">Xuất File</button>
                    </div>
                </div>
                <table className="w-full bg-white shadow-md rounded">
                    <thead className="bg-gray-100">
                        <tr>
                            <th className="p-2">Mã sản phẩm</th>
                            <th className="p-2">Ảnh sản phẩm</th>
                            <th className="p-2">Tên sản phẩm</th>
                            <th className="p-2">Loại sản phẩm</th>
                            <th className="p-2">Đơn vị</th>
                            <th className="p-2">Tồn kho</th>
                            <th className="p-2">Giá nhập</th>
                            <th className="p-2">Giá bán</th>
                            <th className="p-2">Trạng thái</th>
                            <th className="p-2">Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td className="p-2">SP000025</td>
                            <td className="p-2"><img src="https://www.havietfoods.vn/upload/Pho%20Bo%20(goi).jpg" alt="Ảnh sản phẩm" width="50" /></td>
                            <td className="p-2">Hộp phở bò phở cổ</td>
                            <td className="p-2">Thực phẩm ăn liền</td>
                            <td className="p-2">Thùng</td>
                            <td className="p-2">120</td>
                            <td className="p-2">100,000</td>
                            <td className="p-2">120,000</td>
                            <td className="p-2">Enable</td>
                            <td className="p-2 space-x-2">
                                <button className="bg-green-500 text-white px-2 py-1 rounded">Sửa</button>
                                <button className="bg-red-500 text-white px-2 py-1 rounded">Disable</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default ProductManagementComponent;

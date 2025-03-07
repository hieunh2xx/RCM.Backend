import React from 'react';

const AddProductComponent = () => {
    return (
        <div className="p-4 bg-white rounded-lg shadow-md">
            <h2 className="text-xl font-semibold mb-4">Thêm sản phẩm</h2>
            <div className="grid grid-cols-2 gap-4 mb-4">
                <div>
                    <label className="block mb-1 font-medium">Tên sản phẩm *</label>
                    <input type="text" className="w-full p-2 border rounded" placeholder="Nhập tên sản phẩm" />
                </div>
                <div>
                    <label className="block mb-1 font-medium">Mã sản phẩm/SKU</label>
                    <input type="text" className="w-full p-2 border rounded" placeholder="Nhập mã SKU" />
                </div>
                <div>
                    <label className="block mb-1 font-medium">Khối lượng</label>
                    <input type="number" className="w-full p-2 border rounded" placeholder="0" />
                </div>
                <div>
                    <label className="block mb-1 font-medium">Đơn vị tính</label>
                    <input type="text" className="w-full p-2 border rounded" placeholder="Nhập đơn vị tính" />
                </div>
            </div>
            <div className="mb-4">
                <label className="block mb-1 font-medium">Mô tả sản phẩm</label>
                <textarea className="w-full p-2 border rounded" placeholder="Nhập mô tả sản phẩm"></textarea>
            </div>
            <div className="grid grid-cols-3 gap-4 mb-4">
                <div>
                    <label className="block mb-1 font-medium">Giá bán lẻ</label>
                    <input type="number" className="w-full p-2 border rounded" placeholder="0" />
                </div>
                <div>
                    <label className="block mb-1 font-medium">Giá bán buôn</label>
                    <input type="number" className="w-full p-2 border rounded" placeholder="0" />
                </div>
                <div>
                    <label className="block mb-1 font-medium">Giá nhập</label>
                    <input type="number" className="w-full p-2 border rounded" placeholder="0" />
                </div>
            </div>
            <div className="mb-4">
                <label className="block mb-1 font-medium">Ảnh sản phẩm</label>
                <div className="border-dashed border-2 p-4 rounded-md text-center">
                    + Kéo thả hoặc tải ảnh lên từ thiết bị
                </div>
            </div>
            <div className="flex justify-end space-x-4">
                <button className="px-4 py-2 bg-gray-500 text-white rounded">Thoát</button>
                <button className="px-4 py-2 bg-green-500 text-white rounded">Lưu và in mã vạch</button>
                <button className="px-4 py-2 bg-blue-500 text-white rounded">Lưu</button>
            </div>
        </div>
    );
};

export default AddProductComponent;

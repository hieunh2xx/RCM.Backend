import React, { useEffect, useState } from 'react';
import Select from 'react-select';
import axios from 'axios';

const Filter = ({ onFilterChange }) => {
    const [products, setProducts] = useState([]);
    const [warehouses, setWarehouses] = useState([]);

    const [categories, setCategories] = useState([]);

    const [selectedProduct, setSelectedProduct] = useState(null);
    const [selectedWarehouses, setSelectedWarehouses] = useState([]);
    const [selectedCategories, setSelectedCategories] = useState([]);

    useEffect(() => {
        fetchProducts();
        fetchWarehouses();
        fetchCategories();
    }, []);

    const fetchProducts = async (selectedCategory) => {
        try {
            const response = await axios.get('http://localhost:5000/api/products');

            console.log("📌 API trả về danh sách sản phẩm:", response.data);

            if (!Array.isArray(response.data)) {
                console.error("❌ API không trả về một mảng hợp lệ:", response.data);
                return;
            }

            // Nếu category = "all" => Hiển thị tất cả sản phẩm, nếu không thì lọc theo category đã chọn
            const filteredProducts = selectedCategory === 'all' || selectedCategory.length === 0
                ? response.data
                : response.data.filter(p => selectedCategory.includes(p.category));

            console.log("📌 Sản phẩm sau khi lọc:", filteredProducts);

            // Định dạng sản phẩm theo React-Select
            const formattedProducts = [
                { value: 'all', label: 'Tất cả' },
                ...filteredProducts.map(p => ({ value: p.productsId, label: p.name }))
            ];

            setProducts(formattedProducts);
        } catch (error) {
            console.error("❌ Lỗi khi lấy danh sách sản phẩm:", error);
        }
    };

    const fetchWarehouses = async () => {
        try {
            const response = await axios.get("http://localhost:5000/api/warehouses");

            console.log("Dữ liệu API trả về:", response.data); // Log dữ liệu gốc

            if (!Array.isArray(response.data)) {
                console.error("API không trả về một mảng hợp lệ:", response.data);
                return;
            }

            const formattedData = response.data.map(w => ({
                value: w.warehousesId, // Đảm bảo đúng key
                label: w.name // Kiểm tra field này có đúng không
            }));

            console.log("Dữ liệu sau khi format:", formattedData); // Log dữ liệu đã xử lý

            setWarehouses(formattedData);
        } catch (error) {
            console.error("Lỗi khi lấy danh sách chi nhánh:", error);
        }
    };


    const fetchCategories = async () => {
        try {
            const response = await axios.get('http://localhost:5000/api/products');

            console.log("📌 API trả về danh sách sản phẩm:", response.data);

            if (!Array.isArray(response.data)) {
                console.error("❌ API không trả về một mảng hợp lệ:", response.data);
                return;
            }

            // Lọc category hợp lệ (không null, không undefined, không rỗng) và loại bỏ trùng lặp
            const uniqueCategories = new Set();

            response.data.forEach(product => {
                if (product.category) {
                    uniqueCategories.add(product.category);
                }
            });

            console.log("📌 Danh mục sau khi lọc:", uniqueCategories);

            // Định dạng danh mục theo React-Select
            setCategories([
                { value: 'all', label: 'Tất cả' },
                ...Array.from(uniqueCategories).map(c => ({ value: c, label: c }))
            ]);
        } catch (error) {
            console.error("❌ Lỗi khi lấy danh mục sản phẩm:", error);
        }
    };


    const handleSelectWarehouses = (selectedOptions) => {
        if (!selectedOptions) {
            setSelectedWarehouses([]);
            return;
        }

        setSelectedWarehouses(selectedOptions);
        localStorage.setItem("selectedWarehouses", JSON.stringify(selectedOptions));
    };
    useEffect(() => {
        fetchProducts(selectedCategories.length > 0 ? selectedCategories[0].value : 'all');
    }, [selectedCategories]);

    return (
        <div>
            <h2 className="text-2xl font-bold mb-4">Dashboard Bán Hàng</h2>
            <h3 className="text-lg font-semibold mb-2">Mặt hàng</h3>
            <Select
                options={products}
                value={selectedProduct}
                onChange={setSelectedProduct}
                placeholder="Chọn mặt hàng"
            />
            <h3 className="text-lg font-semibold mb-2">Chi nhánh</h3>
            <Select
                options={warehouses}
                value={selectedWarehouses}
                onChange={handleSelectWarehouses} // Gọi hàm cập nhật state
                isMulti
                placeholder="Chọn chi nhánh"
                menuPortalTarget={document.body}
                styles={{ menuPortal: base => ({ ...base, zIndex: 9999 }) }}
            />
            <h3 className="text-lg font-semibold mb-2">Nhóm hàng hóa</h3>
            <Select
                options={categories}
                value={selectedCategories} // Hiển thị danh mục đã chọn
                onChange={setSelectedCategories}
                isMulti
                placeholder="Chọn nhóm hàng"
                menuPortalTarget={document.body}
                styles={{ menuPortal: base => ({ ...base, zIndex: 9999 }) }}
            />
        </div>
    );
};

export default Filter;

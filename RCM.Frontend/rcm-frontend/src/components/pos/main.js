import React, { useState } from 'react';
import { Container, Row, Col, Form, Button } from 'react-bootstrap';
import Cart from './cart';
import Calculator from './calculator';
import { BsX } from 'react-icons/bs';
import './main.css';

const productList = [
    { id: 1, name: 'Hoa Yến Mạch', price: 170000, unit: 'Bó', barcode: '123456789' },
    { id: 2, name: 'Lọ hoa gốm trang trí - S', price: 600000, unit: 'Chiếc', barcode: '987654321' },
    { id: 3, name: 'Bình hoa thủy tinh', price: 250000, unit: 'Chiếc', barcode: '555555555' },
    { id: 4, name: 'Hoa cẩm tú cầu', price: 200000, unit: 'Bó', barcode: '111222333' }
];

const Main = () => {
    const [invoices, setInvoices] = useState({ 'Hóa đơn 1': [] });
    const [currentInvoice, setCurrentInvoice] = useState('Hóa đơn 1');
    const [searchText, setSearchText] = useState('');
    const [showSuggestions, setShowSuggestions] = useState(false);

    const handleUpdateCart = (updatedCart) => {
        setInvoices((prevInvoices) => ({
            ...prevInvoices,
            [currentInvoice]: updatedCart
        }));
    };

    const handleSearchChange = (e) => {
        setSearchText(e.target.value);
        setShowSuggestions(true);
    };

    const handleAddProductToCart = (product) => {
        const existingProduct = invoices[currentInvoice]?.find(item => item.id === product.id);
        let updatedCart;

        if (existingProduct) {
            updatedCart = invoices[currentInvoice].map(item =>
                item.id === product.id
                    ? { ...item, quantity: (parseFloat(item.quantity) + 1).toString() }
                    : item
            );
        } else {
            updatedCart = [...invoices[currentInvoice], { ...product, quantity: '1' }];
        }

        handleUpdateCart(updatedCart);
        setSearchText('');
        setShowSuggestions(false);
    };

    const handleClickOutside = (e) => {
        if (!e.target.closest('.search-suggestions') && !e.target.closest('.form-control')) {
            setShowSuggestions(false);
        }
    };

    const filteredProducts = productList.filter(
        product =>
            product.name.toLowerCase().includes(searchText.toLowerCase()) ||
            product.barcode === searchText
    );

    // Quản lý hóa đơn
    const getNextInvoiceNumber = () => {
        const usedNumbers = Object.keys(invoices)
            .map((name) => parseInt(name.replace('Hóa đơn ', '')))
            .sort((a, b) => a - b);

        for (let i = 1; i <= usedNumbers.length; i++) {
            if (!usedNumbers.includes(i)) {
                return i;
            }
        }
        return usedNumbers.length + 1;
    };

    const handleAddNewInvoice = () => {
        const newInvoiceNumber = getNextInvoiceNumber();
        const newInvoiceId = `Hóa đơn ${newInvoiceNumber}`;
        setInvoices({ ...invoices, [newInvoiceId]: [] });
        setCurrentInvoice(newInvoiceId);
    };

    // Xóa hóa đơn
const handleRemoveInvoice = (invoiceId) => {
    const updatedInvoices = { ...invoices };
    delete updatedInvoices[invoiceId];

    if (Object.keys(updatedInvoices).length === 0) {
        // Nếu xóa hết hóa đơn, tạo hóa đơn mới trống
        const newInvoiceId = 'Hóa đơn 1';
        updatedInvoices[newInvoiceId] = [];
        setCurrentInvoice(newInvoiceId);
    } else {
        // Chuyển sang hóa đơn khác nếu hóa đơn hiện tại bị xóa
        const remainingInvoices = Object.keys(updatedInvoices);
        setCurrentInvoice(remainingInvoices[0]);
    }

    setInvoices(updatedInvoices);
};


    const handleSwitchInvoice = (invoiceId) => {
        setCurrentInvoice(invoiceId);
    };

    const currentCartData = invoices[currentInvoice] || [];

    return (
        <Container fluid className='page-body' onClick={handleClickOutside}>
            <Row className='tool-bar align-items-center'>
                <Col md={4} className="mt-2 position-relative">
                    <Form.Control
                        type="text"
                        placeholder="Tìm kiếm sản phẩm hoặc quét mã vạch..."
                        value={searchText}
                        onChange={handleSearchChange}
                        onFocus={() => setShowSuggestions(true)}
                        className="form-control h-75"
                    />
                    {showSuggestions && searchText && (
                        <div
                            className="search-suggestions border bg-white shadow mt-2 position-absolute w-100"
                            style={{
                                maxHeight: '200px',
                                overflowY: 'auto',
                                zIndex: 10,
                                borderRadius: '4px'
                            }}
                        >
                            {filteredProducts.map(product => (
                                <div
                                    key={product.id}
                                    onClick={() => handleAddProductToCart(product)}
                                    className="p-2 border-bottom cursor-pointer hover-bg-light"
                                >
                                    {product.name} - {product.price.toLocaleString()} VND
                                </div>
                            ))}
                        </div>
                    )}
                </Col>

                <Col md={8} className="d-flex justify-content-end align-items-center">
                    <div className="d-flex flex-wrap align-items-center">
                        {Object.keys(invoices).map((invoiceId) => (
                            <div key={invoiceId} className="invoice-wrapper me-2">
                                <Button
                                    variant={invoiceId === currentInvoice ? 'primary' : 'outline-primary'}
                                    className="invoice-button d-flex align-items-center"
                                    onClick={() => handleSwitchInvoice(invoiceId)}
                                >
                                    <span>{invoiceId}</span>
                                    <BsX
                                        className="invoice-close-btn ms-2"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handleRemoveInvoice(invoiceId);
                                        }}
                                    />
                                </Button>
                            </div>
                        ))}
                        <Button variant="success" onClick={handleAddNewInvoice}>
                            Thêm +
                        </Button>
                    </div>
                </Col>
            </Row>

            <Row>
                <Col md={8} className='mt-2'>
                    <Container className='item-card left-item'>
                        <Cart cartData={currentCartData} onUpdateCart={handleUpdateCart} />
                    </Container>
                </Col>
                <Col md={4} className='mt-2'>
                    <Container className='item-card right-item'>
                        <Calculator cartData={currentCartData} />
                    </Container>
                </Col>
            </Row>
        </Container>
    );
};

export default Main;

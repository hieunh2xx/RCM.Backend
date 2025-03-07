import React, { useEffect, useState } from 'react';
import { Card, Form, Button, ToggleButton, ToggleButtonGroup } from 'react-bootstrap';

const Calculator = ({ cartData }) => {
    const [totalItems, setTotalItems] = useState(0);
    const [totalPrice, setTotalPrice] = useState(0);
    const [paymentMethod, setPaymentMethod] = useState('cash'); // Mặc định là "Tiền mặt"
    const [cashGiven, setCashGiven] = useState(0); // Số tiền khách đưa
    const [change, setChange] = useState(0); // Tiền thối lại

    useEffect(() => {
        const totalItems = cartData.reduce((total, item) => total + parseFloat(item.quantity || 0), 0);
        setTotalItems(totalItems);

        const totalPrice = cartData.reduce((total, item) =>
            total + (parseFloat(item.price || 0) * parseFloat(item.quantity || 0)), 0
        );
        setTotalPrice(totalPrice);
    }, [cartData]);

    const totalAfterDiscount = totalPrice; // Không còn giảm giá, tổng tiền = tổng giá trị sản phẩm

    // 🛠 Gợi ý số tiền khách đưa **theo mệnh giá hợp lý**
    const generateSuggestedAmounts = (amount) => {
    let suggested = [amount]; // Luôn gợi ý số tiền chính xác cần trả

    // Danh sách mệnh giá tiền Việt Nam
    const denominations = [1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000];

    // 🔹 **Nếu tổng tiền không tròn, tìm tổ hợp gần nhất**
    if (amount % 1000 !== 0) {
        suggested.push(amount + (1000 - (amount % 1000))); // Làm tròn lên 1K
    }
    if (amount % 2000 !== 0) {
        suggested.push(amount + (2000 - (amount % 2000))); // Làm tròn lên 2K
    }
    if (amount % 5000 !== 0) {
        suggested.push(amount + (5000 - (amount % 5000))); // Làm tròn lên 5K
    }

    // 🔹 **Thêm tất cả các tổ hợp tiền khách có thể đưa**
    denominations.forEach((denom) => {
        if (denom > amount) suggested.push(denom); // Các mệnh giá lớn hơn số tiền cần trả
        if (denom < amount && amount - denom <= 100000) {
            suggested.push(denom + 10000); // Gợi ý mệnh giá + 10K
            suggested.push(denom + 20000); // Gợi ý mệnh giá + 20K
            suggested.push(denom + 50000); // Gợi ý mệnh giá + 50K
            suggested.push(denom + 100000); // Gợi ý mệnh giá + 100K
        }
    });

    return [...new Set(suggested)].sort((a, b) => a - b);
};


    const suggestedAmounts = generateSuggestedAmounts(totalAfterDiscount);


    // Khi khách nhập số tiền đưa
    const handleCashGivenChange = (e) => {
        const value = parseFloat(e.target.value) || 0;
        setCashGiven(value);
        setChange(Math.max(value - totalAfterDiscount, 0));
    };

    // Khi chọn số tiền gợi ý
    const handleSuggestedAmount = (amount) => {
        setCashGiven(amount);
        setChange(Math.max(amount - totalAfterDiscount, 0));
    };

    return (
        <Card className="p-3">
            <Form>
                <Form.Group className="mb-2">
                    <Form.Label>Tổng tiền hàng ({totalItems} sản phẩm)</Form.Label>
                    <div className="text-end fw-bold fs-5 text-primary">
                        {(totalPrice || 0).toLocaleString()} VND
                    </div>
                </Form.Group>

                <Form.Group className="mb-3">
                    <Form.Label>Khách cần trả</Form.Label>
                    <div className="text-end fw-bold fs-4 text-success">
                        {(totalAfterDiscount || 0).toLocaleString()} VND
                    </div>
                </Form.Group>

                <Form.Group className="mb-3">
                    <Form.Label>Phương thức thanh toán</Form.Label>
                    <ToggleButtonGroup
                        type="radio"
                        name="paymentMethod"
                        value={paymentMethod}
                        onChange={(val) => setPaymentMethod(val)} // Cập nhật trực tiếp trạng thái
                        className="w-100"
                    >
                        <ToggleButton id="btn-cash" variant={paymentMethod === "cash" ? "primary" : "outline-primary"} value="cash">
                            Tiền mặt
                        </ToggleButton>
                        <ToggleButton id="btn-transfer" variant={paymentMethod === "transfer" ? "primary" : "outline-secondary"} value="transfer">
                            Chuyển khoản
                        </ToggleButton>
                    </ToggleButtonGroup>
                </Form.Group>



                {/* Chỉ hiển thị khi thanh toán bằng tiền mặt */}
                {paymentMethod === 'cash' && (
                    <Form.Group className="mb-3">
                        <Form.Label>Số tiền khách đưa</Form.Label>
                        <div className="d-flex flex-wrap gap-2 justify-content-start mb-2">
                            {suggestedAmounts.map((amount, index) => (
                                <Button
                                    key={amount}
                                    variant="outline-secondary"
                                    className="px-3 py-2 text-center rounded"
                                    style={{
                                        minWidth: "90px",
                                        fontSize: "14px",
                                        marginBottom: index < 5 ? "5px" : "0",
                                    }}
                                    onClick={() => handleSuggestedAmount(amount)}
                                >
                                    {amount.toLocaleString()}
                                </Button>
                            ))}
                        </div>
                        <Form.Control
                            type="number"
                            min="0"
                            value={cashGiven}
                            onChange={handleCashGivenChange}
                            placeholder="Nhập số tiền khách đưa"
                        />
                    </Form.Group>
                )}

                {/* Chỉ hiển thị khi thanh toán bằng tiền mặt */}
                {paymentMethod === 'cash' && (
                    <Form.Group className="mb-3">
                        <Form.Label>Tiền thối lại</Form.Label>
                        <div className="text-end fw-bold fs-4 text-danger">
                            {(change || 0).toLocaleString()} VND
                        </div>
                    </Form.Group>
                )}

                <Button
                    variant="primary"
                    className="w-100 py-2 fs-5"
                >
                    Thanh toán
                </Button>
            </Form>
        </Card>
    );
};

export default Calculator;

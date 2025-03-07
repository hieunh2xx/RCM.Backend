import React, { useEffect, useState } from 'react';
import { Button, Container, Row, Col, Card, InputGroup, FormControl } from 'react-bootstrap';
import { BsTrash } from 'react-icons/bs';

const Cart = ({ cartData, onUpdateCart }) => {
    const [cart, setCart] = useState(cartData);

    useEffect(() => {
        setCart(cartData);
    }, [cartData]);

    const handleQuantityChange = (id, value) => {
        let numericValue = value.replace(/[^0-9.]/g, '');

        const dotCount = (numericValue.match(/\./g) || []).length;
        if (dotCount > 1) {
            return;
        }

        if (numericValue.includes('.')) {
            const [integerPart, decimalPart] = numericValue.split('.');
            if (decimalPart.length > 3) {
                return;
            }
        }

        const updatedCart = cart.map(item =>
            item.id === id ? { ...item, quantity: numericValue } : item
        );

        setCart(updatedCart);
        onUpdateCart(updatedCart);
    };

    const handleRemoveItem = (id) => {
        const updatedCart = cart.filter(item => item.id !== id);
        setCart(updatedCart);
        onUpdateCart(updatedCart);
    };

    return (
        <Container className="mt-4">
            {cart.map(item => (
                <Card key={item.id} className="mb-3 p-2">
                    <Row className="align-items-center">
                        <Col xs={3} className="fw-bold">{item.name}</Col>
                        <Col xs={1} className="text-center text-muted">{item.unit}</Col>
                        <Col xs={3}>
                            <InputGroup className="d-flex align-items-center">
                                <Button
                                    variant="outline-secondary"
                                    onClick={() => handleQuantityChange(item.id, (parseFloat(item.quantity) - 1).toString())}
                                >-</Button>

                                <FormControl
                                    type="text"
                                    inputMode="decimal"
                                    value={item.quantity}
                                    onChange={(e) => handleQuantityChange(item.id, e.target.value)}
                                    className="mx-2 text-center quantity-input"
                                />

                                <Button
                                    variant="outline-secondary"
                                    onClick={() => handleQuantityChange(item.id, (parseFloat(item.quantity) + 1).toString())}
                                >+</Button>
                            </InputGroup>
                        </Col>
                        <Col xs={2} className="text-end">{(item.price ?? 0).toLocaleString()} VND</Col>
                        <Col xs={2} className="text-end fw-bold">
                            {((parseFloat(item.price) || 0) * (parseFloat(item.quantity) || 0)).toLocaleString()} VND
                        </Col>
                        <Col xs={1} className="text-center">
                            <Button
                                variant="outline-danger"
                                onClick={() => handleRemoveItem(item.id)}
                            >
                                <BsTrash size={20} variant="outline-danger" />
                            </Button>
                        </Col>
                    </Row>
                </Card>
            ))}
        </Container>
    );
};

export default Cart;

import React, { useState } from "react";
import { Input, Button, Form, message } from "antd";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const RegisterStaff = () => {
  const [form] = Form.useForm();
  const navigate = useNavigate();

  const onFinish = async (values) => {
    try {
      const response = await fetch(
        "http://localhost:5000/api/Staff/add-account",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ ...values, role: 2 }),
        }
      );

      if (response.ok) {
        toast.success("Tạo tài khoản nhân viên thành công!");
        form.resetFields();
        navigate("/login");
      } else {
        const errorData = await response.json();
        toast.error(errorData.message || "Đã có lỗi xảy ra");
      }
    } catch (error) {
      message.error("Lỗi kết nối đến server");
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="bg-white p-8 rounded-2xl shadow-md w-96">
        <h2>Đăng Ký Nhân Viên</h2>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item
            label="Tên đăng nhập"
            name="username"
            rules={[{ required: true, message: "Vui lòng nhập tên đăng nhập" }]}
          >
            <Input placeholder="Nhập tên đăng nhập" className="p-3" />
          </Form.Item>

          <Form.Item
            label="Mật khẩu"
            name="password"
            rules={[{ required: true, message: "Vui lòng nhập mật khẩu" }]}
          >
            <Input.Password placeholder="Nhập mật khẩu" className="p-3" />
          </Form.Item>

          <Form.Item
            label="ID Nhân viên"
            name="employeeId"
            rules={[{ required: true, message: "Vui lòng nhập ID nhân viên" }]}
          >
            <Input
              type="text"
              placeholder="Nhập ID nhân viên"
              className="p-3"
            />
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              className="w-full bg-blue-600 text-white p-8 rounded-md font-semibold hover:bg-blue-700"
            >
              ĐĂNG KÝ
            </Button>
          </Form.Item>
        </Form>
      </div>
    </div>
  );
};

export default RegisterStaff;

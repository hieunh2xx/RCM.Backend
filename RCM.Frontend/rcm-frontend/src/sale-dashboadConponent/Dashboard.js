import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Bar } from 'react-chartjs-2';

const Dashboard = ({ filters }) => {
    /* const [chartData, setChartData] = useState(null);
 
     useEffect(() => {
         if (filters) {
             fetchDashboardData();
         }
     }, [filters]);
 
     const fetchDashboardData = async () => {
         try {
             const response = await axios.post('http://localhost:5000/api/sales/dashboard', filters);
             const data = response.data;
 
             const labels = data.map(item => item.ProductName);
             const quantities = data.map(item => item.QuantitySold);
             const sales = data.map(item => item.TotalSales);
 
             setChartData({
                 labels,
                 datasets: [
                     {
                         label: 'Số lượng bán',
                         data: quantities,
                         backgroundColor: 'rgba(75, 192, 192, 0.6)',
                     },
                     {
                         label: 'Doanh thu (VNĐ)',
                         data: sales,
                         backgroundColor: 'rgba(255, 99, 132, 0.6)',
                     }
                 ]
             });
         } catch (error) {
             console.error('Lỗi khi lấy dữ liệu biểu đồ:', error);
         }
     };*/

    return (
        /*
        <div className="p-4 w-full">
           
            {chartData ? (
                <Bar 
                    data={chartData} 
                    options={{
                        responsive: true,
                        scales: {
                            y: {
                                beginAtZero: true
                            }
                        }
                    }}
                />
            ) : (
                <p>Chưa có dữ liệu để hiển thị biểu đồ.</p>
            )}
        </div>
    */null
    );
};
export default Dashboard;

import React, { useState } from 'react';
import Filter from './Filter';
import Dashboard from './Dashboard';
import Header from '../headerComponent/header';

const SalesChartPage = () => {
    const [filters, setFilters] = useState({});

    const handleFilterChange = (newFilters) => {
        setFilters(newFilters);
    };

    return (
        <div className="flex flex-col min-h-screen">
            {/* Hiển thị Header ở trên cùng */}
            <Header />
            {/* Nội dung chính nằm bên dưới Header */}
            <main className="flex bg-gray-100 flex-grow">
                <div className="w-1/4 bg-white p-4 shadow-md">
                    <Filter onFilterChange={handleFilterChange} />
                </div>
                <div className="w-3/4 p-4">
                    <Dashboard filters={filters} />
                </div>
            </main>
        </div>
    );
};

export default SalesChartPage;

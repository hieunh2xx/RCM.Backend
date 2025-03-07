import React, { useState } from 'react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { SearchIcon } from 'lucide-react';

const SearchComponent = ({ placeholder = 'Tìm kiếm...', onSearch }) => {
    const [searchTerm, setSearchTerm] = useState('');

    const handleSearch = () => {
        if (onSearch) {
            onSearch(searchTerm);
        }
    };

    const handleKeyPress = (e) => {
        if (e.key === 'Enter') {
            handleSearch();
        }
    };

    return (
        <div className="flex items-center space-x-2 p-2">
            <Input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                onKeyPress={handleKeyPress}
                placeholder={placeholder}
                className="w-full border border-gray-300 rounded-md p-2"
            />
            <Button onClick={handleSearch} className="flex items-center space-x-1 bg-blue-500 text-white p-2 rounded-md">
                <SearchIcon className="w-4 h-4" />
                <span>Tìm</span>
            </Button>
        </div>
    );
};

export default SearchComponent;

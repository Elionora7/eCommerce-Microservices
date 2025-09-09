import { useMemo, useState } from 'react';
import { useProducts } from '@/features/products/ProductHooks';
import { useOrdersByUserId } from '@/features/orders/OrderHooks';
import ProductCard from '@/features/products/components/ProductCard';
import ShoppingCart from '@/components/ShoppingCart';
import { useCart } from '@/contexts/CartContext';
import { useNavigate } from 'react-router-dom';
import { formatCurrency } from '@/utils/formatCurrency';

type SortKey = 'name' | 'category' | 'price' | 'stock';
type SortDir = 'asc' | 'desc';

// simple client-side paginator
function paginate<T>(arr: T[], page: number, pageSize: number) {
  const start = (page - 1) * pageSize;
  return arr.slice(start, start + pageSize);
}

export default function DashboardPage() {
  const navigate = useNavigate();
  const { state: cartState, dispatch: cartDispatch } = useCart();
  
  // Get current user ID
  const userData = localStorage.getItem('user');
  const currentUserId = userData ? JSON.parse(userData).userID : null;

  const { data: products = [], isLoading, isError, error } = useProducts();
  const { data: userOrdersData = [] } = useOrdersByUserId(currentUserId || '');
  // Use the data or fallback to empty array
  const userOrders = userOrdersData || [];

  // Search, sorting, pagination state
  const [search, setSearch] = useState('');
  const [sortKey, setSortKey] = useState<SortKey>('name');
  const [sortDir] = useState<SortDir>('asc');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(8);

  // Filter and sort products
  const filteredProducts = useMemo(() => {
    const term = search.trim().toLowerCase();
    if (!term) return products;
    return products.filter((p) => {
      const name = p.name.toLowerCase();
      const cat = p.category.toLowerCase();
      return name.includes(term) || cat.includes(term);
    });
  }, [products, search]);

  const sortedProducts = useMemo(() => {
    const arr = [...filteredProducts];
    arr.sort((a, b) => {
      let va: string | number;
      let vb: string | number;
      switch (sortKey) {
        case 'name':
          va = a.name.toLowerCase();
          vb = b.name.toLowerCase();
          break;
        case 'category':
          va = a.category.toLowerCase();
          vb = b.category.toLowerCase();
          break;
        case 'price':
          va = a.unitPrice;
          vb = b.unitPrice;
          break;
        case 'stock':
          va = a.quantity;
          vb = b.quantity;
          break;
        default:
          va = 0;
          vb = 0;
      }
      if (va < vb) return sortDir === 'asc' ? -1 : 1;
      if (va > vb) return sortDir === 'asc' ? 1 : -1;
      return 0;
    });
    return arr;
  }, [filteredProducts, sortKey, sortDir]);

  // Pagination
  const pageCount = Math.max(1, Math.ceil(sortedProducts.length / pageSize));
  const safePage = Math.min(page, pageCount);
  const visibleProducts = paginate(sortedProducts, safePage, pageSize);

  function handleLogout() {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('refreshTokenExpiryTime');
    localStorage.removeItem('user');
    localStorage.removeItem('userID');
    navigate('/login');
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">E-Commerce Store</h1>
              <p className="text-sm text-gray-600">Browse our amazing products</p>
            </div>
            
            <div className="flex items-center space-x-4">
              {/* Shopping Cart Button */}
              <button
                onClick={() => cartDispatch({ type: 'TOGGLE_CART' })}
                className="relative p-2 text-gray-600 hover:text-blue-600 transition-colors"
              >
                <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
                {cartState.cart.itemCount > 0 && (
                  <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center">
                    {cartState.cart.itemCount}
                  </span>
                )}
              </button>

              {/* My Orders Button */}
              <button
                onClick={() => navigate('/orders')}
                className="bg-gray-100 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-200 transition flex items-center"
              >
                <svg className="h-5 w-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                 My Orders ({currentUserId ? userOrders.length : 0})
              </button>

              {/* Profile Button */}
              <button
                onClick={() => navigate('/profile')}
                className="bg-gray-100 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-200 transition flex items-center"
              >
                <svg className="h-5 w-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                Profile
              </button>

              {/* Logout Button */}
              <button
                onClick={handleLogout}
                className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition flex items-center"
              >
                <svg className="h-5 w-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                Logout
              </button>
            </div>
          </div>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Welcome Card */}
        <div className="bg-gradient-to-r from-blue-500 to-indigo-600 rounded-2xl p-6 text-white mb-8 shadow-lg">
          <h2 className="text-2xl font-bold mb-3">Product Catalog</h2>
          <p className="text-blue-100">
            {sortedProducts.length} product(s) found
            {search ? ` • filtered by "${search}"` : ''}
          </p>
          <p className="text-blue-100 text-sm mt-2">
            Cart total: {formatCurrency(cartState.cart.total)} • {cartState.cart.itemCount} item(s)
          </p>
        </div>

        {/* Search and Filter Controls */}
        <div className="bg-white rounded-2xl shadow-lg p-6 mb-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="md:col-span-2">
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                <input
                  type="text"
                  value={search}
                  onChange={(e) => {
                    setSearch(e.target.value);
                    setPage(1);
                  }}
                  placeholder="Search products by name or category..."
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
                />
              </div>
            </div>
            
            <div className="flex gap-4">
              <select
                value={sortKey}
                onChange={(e) => {
                  setSortKey(e.target.value as SortKey);
                  setPage(1);
                }}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
              >
                <option value="name">Sort by Name</option>
                <option value="category">Sort by Category</option>
                <option value="price">Sort by Price</option>
                <option value="stock">Sort by Stock</option>
              </select>
              
              <select
                value={pageSize}
                onChange={(e) => {
                  setPageSize(Number(e.target.value));
                  setPage(1);
                }}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
              >
                <option value={8}>8 per page</option>
                <option value={12}>12 per page</option>
                <option value={24}>24 per page</option>
              </select>
            </div>
          </div>
        </div>

        {/* Products Grid */}
        <div className="bg-white rounded-2xl shadow-lg p-6 mb-8">
          {isLoading ? (
            <div className="flex justify-center items-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
          ) : isError ? (
            <div className="text-center py-12">
              <p className="text-red-600">Failed to load products. Please try again later.</p>
              <p className="text-sm text-gray-600 mt-2">{error?.message}</p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {visibleProducts.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </div>
              
              {visibleProducts.length === 0 && (
                <div className="text-center py-12">
                  <p className="text-gray-600">No products found matching your criteria.</p>
                </div>
              )}
            </>
          )}
        </div>

        {/* Pagination */}
        {!isLoading && !isError && sortedProducts.length > 0 && (
          <div className="bg-white rounded-2xl shadow-lg p-6">
            <div className="flex items-center justify-between">
              <div className="text-sm text-gray-600">
                Showing {((safePage - 1) * pageSize) + 1} to {Math.min(safePage * pageSize, sortedProducts.length)} of {sortedProducts.length} products
              </div>
              
              <div className="flex items-center gap-2">
                <button
                  disabled={safePage <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  className="px-4 py-2 rounded-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
                >
                  Previous
                </button>
                
                <span className="px-3 py-2 text-sm text-gray-600">
                  Page {safePage} of {pageCount}
                </span>
                
                <button
                  disabled={safePage >= pageCount}
                  onClick={() => setPage((p) => Math.min(pageCount, p + 1))}
                  className="px-4 py-2 rounded-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
                >
                  Next
                </button>
              </div>
            </div>
          </div>
        )}
      </main>

      {/* Shopping Cart Modal */}
      <ShoppingCart />
    </div>
  );
}
import { useParams } from 'react-router-dom';
import { useProduct } from '@/features/products/ProductHooks';
import { useCart } from '@/contexts/CartContext';
import { formatCurrency } from '@/utils/formatCurrency';
import { useNavigate } from 'react-router-dom';

export default function ProductDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: product, isLoading, isError } = useProduct(id || '');
  const { state, dispatch } = useCart();

  // Check if this product is in the cart
  const cartItem = state.cart.items.find(item => item.productID === product?.id);
  const quantityInCart = cartItem?.quantity || 0;

const handleAddToCart = () => {
  if (!product) return;
  
  const quantityToAdd = 1;
  const totalPriceForItems = product.unitPrice * quantityToAdd;
  
  dispatch({
    type: 'ADD_ITEM',
    payload: {
      productID: product.id,
      name: product.name,
      unitPrice: product.unitPrice,
      quantity: quantityToAdd,
      totalPrice: totalPriceForItems,  
      imgUrl: product.imgUrl,
      maxStock: product.quantity
    }
  });
};

  const handleIncreaseQuantity = () => {
    if (!product) return;
    
    dispatch({
      type: 'UPDATE_QUANTITY',
      payload: { productID: product.id, quantity: quantityInCart + 1 }
    });
  };

  const handleDecreaseQuantity = () => {
    if (!product) return;
    
    dispatch({
      type: 'UPDATE_QUANTITY',
      payload: { productID: product.id, quantity: quantityInCart - 1 }
    });
  };

  if (isLoading) return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="animate-pulse">
        <div className="h-8 bg-gray-200 rounded w-1/3 mb-4"></div>
        <div className="h-4 bg-gray-200 rounded w-1/4 mb-8"></div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="h-80 bg-gray-200 rounded"></div>
          <div className="space-y-4">
            <div className="h-6 bg-gray-200 rounded"></div>
            <div className="h-4 bg-gray-200 rounded w-2/3"></div>
            <div className="h-8 bg-gray-200 rounded w-1/2"></div>
          </div>
        </div>
      </div>
    </div>
  );

  if (isError) return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
        <h2 className="text-xl font-semibold text-red-800 mb-2">Error loading product</h2>
        <p className="text-red-600">Could not load product details. Please try again later.</p>
      </div>
    </div>
  );

  if (!product) return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-6 text-center">
        <h2 className="text-xl font-semibold text-yellow-800 mb-2">Product not found</h2>
        <p className="text-yellow-600">The product you're looking for doesn't exist.</p>
      </div>
    </div>
  );

  return (
    <div className="max-w-4xl mx-auto p-6">
      {/* Back Button */}
      <button
        onClick={() => navigate(-1)}
        className="flex items-center text-blue-600 hover:text-blue-800 mb-6"
      >
        <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
        </svg>
        Back to Products
      </button>

      <div className="bg-white rounded-2xl shadow-lg p-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Product Image */}
          <div>
            <img
              src={product.imgUrl || '/placeholder.jpg'}
              alt={product.name}
              className="w-full h-96 object-cover rounded-lg"
            />
          </div>

          {/* Product Info */}
          <div className="space-y-6">
            <div>
              <h1 className="text-3xl font-bold text-gray-800">{product.name}</h1>
              <p className="text-gray-500 mt-2 text-lg">{product.category}</p>
            </div>

            <div className="space-y-2">
              <p className="text-3xl font-bold text-blue-600">
                {formatCurrency(product.unitPrice)}
              </p>
              
              {/* Show quantity in cart if added, otherwise show out of stock if applicable */}
              {quantityInCart > 0 && (
                <div className="flex items-center space-x-2">
                  <span className="text-lg text-gray-600">In cart:</span>
                  <span className="text-lg font-semibold text-blue-600">{quantityInCart}</span>
                </div>
              )}
              
              {product.quantity === 0 && (
                <p className="text-lg text-red-600">Out of stock</p>
              )}
            </div>

            {/* Quantity Controls (only show if product is in cart) */}
            {quantityInCart > 0 && (
              <div className="flex items-center space-x-4">
                <span className="text-gray-700">Quantity:</span>
                <div className="flex items-center space-x-2">
                  <button
                    onClick={handleDecreaseQuantity}
                    className="w-8 h-8 bg-gray-200 rounded-full flex items-center justify-center hover:bg-gray-300 transition-colors"
                  >
                    -
                  </button>
                  <span className="w-8 text-center font-semibold">{quantityInCart}</span>
                  <button
                    onClick={handleIncreaseQuantity}
                    disabled={quantityInCart >= product.quantity}
                    className="w-8 h-8 bg-gray-200 rounded-full flex items-center justify-center hover:bg-gray-300 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    +
                  </button>
                </div>
              </div>
            )}

            {/* Action buttons */}
            <div className="space-y-4">
              {quantityInCart === 0 ? (
                <button
                  onClick={handleAddToCart}
                  disabled={product.quantity === 0}
                  className="w-full bg-blue-600 text-white py-4 px-6 rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors text-lg font-semibold"
                >
                  {product.quantity > 0 ? 'Add to Cart 🛒' : 'Out of Stock'}
                </button>
              ) : (
                <button
                  onClick={() => dispatch({ type: 'REMOVE_ITEM', payload: product.id })}
                  className="w-full bg-red-600 text-white py-4 px-6 rounded-lg hover:bg-red-700 transition-colors text-lg font-semibold"
                >
                  Remove from Cart
                </button>
              )}
            </div>

            {/* Additional Info */}
            <div className="border-t pt-4 space-y-2 text-sm text-gray-600">
              <div className="flex justify-between">
                <span>Product ID:</span>
                <span className="font-mono">{product.id}</span>
              </div>
              <div className="flex justify-between">
                <span>Category:</span>
                <span>{product.category}</span>
              </div>
              <div className="flex justify-between">
                <span>Status:</span>
                <span className={product.quantity > 0 ? 'text-green-600' : 'text-red-600'}>
                  {product.quantity > 0 ? 'Available' : 'Out of Stock'}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
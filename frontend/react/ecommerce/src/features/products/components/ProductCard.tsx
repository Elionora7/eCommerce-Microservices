import { Link } from 'react-router-dom';
import { useCart } from '@/contexts/CartContext';
import { formatCurrency } from '@/utils/formatCurrency';
import type { Product } from '../ProductTypes';

interface ProductCardProps {
  product: Product;
}

export default function ProductCard({ product }: ProductCardProps) {
  const { state, dispatch } = useCart();

  // Check if this product is already in the cart
  const cartItem = state.cart.items.find(item => item.productID === product.id);
  const quantityInCart = cartItem?.quantity || 0;

  const handleAddToCart = (e: React.MouseEvent) => {
    e.preventDefault(); // Prevent navigation when clicking Add to Cart
    e.stopPropagation(); // Stop event bubbling
    
    dispatch({
      type: 'ADD_ITEM',
      payload: {
        productID: product.id,
        name: product.name,
        unitPrice: product.unitPrice,
        quantity: 1,
        totalPrice: product.unitPrice,
        imgUrl: product.imgUrl,
        maxStock: product.quantity
      }
    });
  };

  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow group">
      {/* Clickable Image and Title */}
      <Link to={`/products/${product.id}`} className="block hover:no-underline">
        <div className="relative overflow-hidden">
          <img
            src={product.imgUrl || '/placeholder.jpg'}
            alt={product.name}
            className="w-full h-48 object-cover group-hover:scale-105 transition-transform duration-300"
          />
        </div>
        
        <div className="p-4">
          <h3 className="font-semibold text-lg mb-2 text-gray-800 group-hover:text-blue-600 transition-colors">
            {product.name}
          </h3>
          <p className="text-gray-600 text-sm mb-2">{product.category}</p>
        </div>
      </Link>

      {/* Product info and Add to Cart */}
      <div className="p-4 pt-0">
        <div className="flex items-center justify-between mb-4">
          <span className="text-2xl font-bold text-blue-600">
            {formatCurrency(product.unitPrice)}
          </span>
          
          {/* Show quantity in cart if added, otherwise show nothing (or out of stock) */}
          {quantityInCart > 0 && (
            <span className="text-sm bg-blue-100 text-blue-800 px-2 py-1 rounded-full">
              {quantityInCart} in cart
            </span>
          )}
          
          {product.quantity === 0 && (
            <span className="text-sm text-red-600">Out of stock</span>
          )}
        </div>

        <button
          onClick={handleAddToCart}
          disabled={product.quantity === 0}
          className="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
        >
          {product.quantity > 0 ? 'Add to Cart' : 'Out of Stock'}
        </button>
      </div>
    </div>
  );
}
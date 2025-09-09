import { useCart } from '@/contexts/CartContext';
import { formatCurrency } from '@/utils/formatCurrency';
import { useCreateOrder } from '@/features/orders/OrderHooks';
import { useNavigate } from 'react-router-dom';

export default function ShoppingCart() {
  const { state, dispatch } = useCart();
  const createOrderMutation = useCreateOrder();
  const navigate = useNavigate();
  const { cart, isOpen } = state;

  const getUserId = () => {
    // Try to get user ID from the correct location
    const userData = localStorage.getItem('user');
    if (userData) {
      try {
        const user = JSON.parse(userData);
        return user.userID; // This is the actual GUID from login
      } catch (error) {
        console.error('Error parsing user data:', error);
      }
    }
    
    // Fallback to the old location (if it exists)
    const oldUserId = localStorage.getItem('userID');
    if (oldUserId && oldUserId !== 'current-user-id') {
      return oldUserId;
    }
    
    return null;
  }

  const handleCheckout = async () => {
    const userId = getUserId();
    const token = localStorage.getItem('token');
    console.log("Retrieved userID:", userId);
    console.log("Retrieved token:", token);

    // Check if user is properly authenticated
    if (!token || !userId) {
      alert('Please login to checkout');
      navigate('/login');
      return;
    }

    console.log("Proceeding with actual userid:", userId);

    const orderData = {
      userID: userId,
      orderDate: new Date().toISOString(),
      orderItems: cart.items.map(item => ({
        productID: item.productID,
        unitPrice: item.unitPrice,
        quantity: item.quantity,
        totalPrice: item.totalPrice,
        imgUrl: item.imgUrl
      }))
    };

    try {
      await createOrderMutation.mutateAsync(orderData);
      dispatch({ type: 'CLEAR_CART' });
      alert('Order placed successfully!');
    } catch (error) {
      console.error('Checkout error:', error);
      alert('Failed to place order. Please try again.');
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden">
      <div className="absolute inset-0 bg-black bg-opacity-50" onClick={() => dispatch({ type: 'TOGGLE_CART' })} />
      
      <div className="absolute right-0 top-0 h-full w-96 bg-white shadow-xl">
        <div className="flex h-full flex-col">
          {/* Header */}
          <div className="flex items-center justify-between border-b p-4">
            <h2 className="text-lg font-semibold">Shopping Cart ({cart.itemCount})</h2>
            <button onClick={() => dispatch({ type: 'TOGGLE_CART' })} className="text-gray-500 hover:text-gray-700">
              ✕
            </button>
          </div>

          {/* Cart Items */}
          <div className="flex-1 overflow-y-auto p-4">
            {cart.items.length === 0 ? (
              <p className="text-center text-gray-500">Your cart is empty</p>
            ) : (
              cart.items.map((item) => (
                <div key={item.productID} className="flex items-center space-x-4 border-b py-4">
                  <img src={item.imgUrl} alt={item.name} className="h-16 w-16 rounded object-cover" />
                  
                  <div className="flex-1">
                    <h3 className="font-medium">{item.name}</h3>
                    <p className="text-sm text-gray-600">{formatCurrency(item.unitPrice)}</p>
                  </div>

                  <div className="flex items-center space-x-2">
                    <button
                      onClick={() => dispatch({
                        type: 'UPDATE_QUANTITY',
                        payload: { productID: item.productID, quantity: item.quantity - 1 }
                      })}
                      className="px-2 py-1 border rounded"
                      disabled={item.quantity <= 1}
                    >
                      -
                    </button>
                    
                    <span className="w-8 text-center">{item.quantity}</span>
                    
                    <button
                      onClick={() => dispatch({
                        type: 'UPDATE_QUANTITY',
                        payload: { productID: item.productID, quantity: item.quantity + 1 }
                      })}
                      className="px-2 py-1 border rounded"
                      disabled={item.quantity >= item.maxStock}
                    >
                      +
                    </button>
                  </div>

                  <div className="text-right">
                    <p className="font-semibold">{formatCurrency(item.totalPrice)}</p>
                    <button
                      onClick={() => dispatch({ type: 'REMOVE_ITEM', payload: item.productID })}
                      className="text-sm text-red-600 hover:text-red-800"
                    >
                      Remove
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>

          {/* Footer */}
          <div className="border-t p-4">
            <div className="flex justify-between text-lg font-semibold mb-4">
              <span>Total:</span>
              <span>{formatCurrency(cart.total)}</span>
            </div>
            
            <button
              onClick={handleCheckout}
              disabled={cart.items.length === 0 || createOrderMutation.isPending}
              className="w-full bg-blue-600 text-white py-3 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {createOrderMutation.isPending ? 'Processing...' : 'Checkout'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
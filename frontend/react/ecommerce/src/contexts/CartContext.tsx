// src/contexts/CartContext.tsx
import React, { createContext, useContext, useReducer, useEffect } from 'react';

export interface CartItem {
  productID: string;
  name: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  imgUrl: string;
  maxStock: number;
}

export interface Cart {
  items: CartItem[];
  total: number;
  itemCount: number;
}

interface CartState {
  cart: Cart;
  isOpen: boolean;
}

type CartAction =
  | { type: 'ADD_ITEM'; payload: CartItem }
  | { type: 'UPDATE_QUANTITY'; payload: { productID: string; quantity: number } }
  | { type: 'REMOVE_ITEM'; payload: string }
  | { type: 'CLEAR_CART' }
  | { type: 'TOGGLE_CART' }
  | { type: 'LOAD_CART'; payload: Cart };

const CartContext = createContext<{
  state: CartState;
  dispatch: React.Dispatch<CartAction>;
} | null>(null);

const cartReducer = (state: CartState, action: CartAction): CartState => {
  switch (action.type) {
    case 'ADD_ITEM': {
      const existingItem = state.cart.items.find(item => item.productID === action.payload.productID);
      
      if (existingItem) {
        const updatedItems = state.cart.items.map(item =>
          item.productID === action.payload.productID
            ? { ...item, quantity: item.quantity + action.payload.quantity }
            : item
        );
        return {
          ...state,
          cart: {
            items: updatedItems,
            total: updatedItems.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0),
            itemCount: updatedItems.reduce((sum, item) => sum + item.quantity, 0)
          }
        };
      }
      
      const newItems = [...state.cart.items, action.payload];
      return {
        ...state,
        cart: {
          items: newItems,
          total: newItems.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0),
          itemCount: newItems.reduce((sum, item) => sum + item.quantity, 0)
        }
      };
    }
    
    case 'UPDATE_QUANTITY': {
      const updatedItems = state.cart.items
        .map(item =>
          item.productID === action.payload.productID
            ? { ...item, quantity: action.payload.quantity }
            : item
        )
        .filter(item => item.quantity > 0);
      
      return {
        ...state,
        cart: {
          items: updatedItems,
          total: updatedItems.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0),
          itemCount: updatedItems.reduce((sum, item) => sum + item.quantity, 0)
        }
      };
    }
    
    case 'REMOVE_ITEM': {
      const updatedItems = state.cart.items.filter(item => item.productID !== action.payload);
      return {
        ...state,
        cart: {
          items: updatedItems,
          total: updatedItems.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0),
          itemCount: updatedItems.reduce((sum, item) => sum + item.quantity, 0)
        }
      };
    }
    
    case 'CLEAR_CART':
      return {
        ...state,
        cart: { items: [], total: 0, itemCount: 0 }
      };
    
    case 'TOGGLE_CART':
      return { ...state, isOpen: !state.isOpen };
    
    case 'LOAD_CART':
      return { ...state, cart: action.payload };
    
    default:
      return state;
  }
};

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, dispatch] = useReducer(cartReducer, {
    cart: { items: [], total: 0, itemCount: 0 },
    isOpen: false
  });

  // Load cart from localStorage on mount
  useEffect(() => {
    const savedCart = localStorage.getItem('cart');
    if (savedCart) {
      dispatch({ type: 'LOAD_CART', payload: JSON.parse(savedCart) });
    }
  }, []);

  // Save cart to localStorage on change
  useEffect(() => {
    localStorage.setItem('cart', JSON.stringify(state.cart));
  }, [state.cart]);

  return (
    <CartContext.Provider value={{ state, dispatch }}>
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};
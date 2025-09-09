import AppRouter from '@/router';
import { CartProvider } from '@/contexts/CartContext';

function App() {
  return (
    <CartProvider>
      <AppRouter />
    </CartProvider>
  );
}

export default App;
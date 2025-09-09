import React from 'react';
import { useProducts } from '../ProductHooks';
import ProductCard from './ProductCard';

const ProductGrid: React.FC = () => {
  const { data: products, isLoading, isError } = useProducts();

  if (isLoading) return <p>Loading products...</p>;
  if (isError) return <p>Failed to load products.</p>;

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
      {products?.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
};

export default ProductGrid;

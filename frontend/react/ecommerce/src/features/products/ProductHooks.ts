import { useQuery } from '@tanstack/react-query';
import { productsAPI } from './ProductService'; 
import type { Product } from './ProductTypes';

export const useProducts = () => {
  return useQuery<Product[], Error>({
    queryKey: ['products'],
    queryFn: productsAPI.list, 
  });
};

export const useProduct = (id: string) => {
  return useQuery<Product, Error>({
    queryKey: ['product', id],
    queryFn: () => productsAPI.getById(id), 
    enabled: !!id, // Only fetch when id exists
  });
};
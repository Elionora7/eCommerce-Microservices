import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ordersAPI } from './OrdersAPI';
import type { Order, OrderAddRequest, OrderUpdateRequest } from './OrderTypes';

// Query keys
export const orderKeys = {
  all: ['orders'] as const,
  lists: () => [...orderKeys.all, 'list'] as const,
  list: (filters: any) => [...orderKeys.lists(), filters] as const,
  details: () => [...orderKeys.all, 'detail'] as const,
  detail: (id: string) => [...orderKeys.details(), id] as const,
  byUserId: (userId: string) => [...orderKeys.all, 'user', userId] as const,
  byProductId: (productId: string) => [...orderKeys.all, 'product', productId] as const,
  byDate: (date: string) => [...orderKeys.all, 'date', date] as const,
};

// Hooks for queries
export const useOrders = () => {
  return useQuery<Order[], Error>({
    queryKey: orderKeys.all,
    queryFn: ordersAPI.list,
  });
};

export const useOrder = (id: string) => {
  return useQuery<Order, Error>({
    queryKey: orderKeys.detail(id),
    queryFn: () => ordersAPI.getById(id),
    enabled: !!id,
  });
};

export const useOrdersByUserId = (userId: string) => {
  const isValidUserId = userId && userId !== 'current-user-id' && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(userId);
  
  return useQuery<Order[], Error>({
    queryKey: orderKeys.byUserId(userId),
    queryFn: () => {
      if (!isValidUserId) {
        throw new Error('Invalid user ID');
      }
      return ordersAPI.getByUserId(userId);
    },
    enabled: !!isValidUserId, // Convert to boolean
    retry: false,
  });
};

export const useOrdersByProductId = (productId: string) => {
  return useQuery<Order[], Error>({
    queryKey: orderKeys.byProductId(productId),
    queryFn: () => ordersAPI.getByProductId(productId),
    enabled: !!productId,
  });
};

export const useOrdersByDate = (date: string) => {
  return useQuery<Order[], Error>({
    queryKey: orderKeys.byDate(date),
    queryFn: () => ordersAPI.getByDate(date),
    enabled: !!date,
  });
};

// Hooks for mutations
export const useCreateOrder = () => {
  const queryClient = useQueryClient();

  return useMutation<Order, Error, OrderAddRequest>({
    mutationFn: ordersAPI.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderKeys.all });
    },
  });
};

export const useUpdateOrder = () => {
  const queryClient = useQueryClient();

  return useMutation<Order, Error, { id: string; data: OrderUpdateRequest }>({
    mutationFn: ({ id, data }) => ordersAPI.update(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.all });
      queryClient.invalidateQueries({ queryKey: orderKeys.detail(data.orderID) });
    },
  });
};

export const useDeleteOrder = () => {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: ordersAPI.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderKeys.all });
    },
  });
};
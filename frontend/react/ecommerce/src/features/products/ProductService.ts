import axios from '@/api/axios';

const BASE = import.meta.env.VITE_API_PATH_PRODUCTS || '/gateway/products';

const mapDtoToProduct = (d: any) => ({
  id: d.productID ?? d.productId ?? d.id ?? '',
  name: d.productName ?? d.name ?? '—',
  category: d.category ?? '—',
  unitPrice: Number.parseFloat(d.unitPrice ?? d.price ?? 0) || 0,
  quantity: Number.parseInt(d.quantityInStock ?? d.stockQuantity ?? d.stock ?? d.quantity ?? 0, 10) || 0,
  imgUrl: d.imgUrl ?? d.imageUrl ?? '', 
});


export const productsAPI = {
  list: async () => {
    const res = await axios.get(BASE);
    return (res.data ?? []).map(mapDtoToProduct);
  },
  
  getById: async (id: string) => {
    const res = await axios.get(`${BASE}/search/product-id/${id}`);
    return mapDtoToProduct(res.data ?? {});
  },
  
  
  create: async (productData: any) => {
    const res = await axios.post(BASE, productData);
    return mapDtoToProduct(res.data);
  },
  
  update: async (id: string, productData: any) => {
    const res = await axios.put(`${BASE}/${id}`, productData);
    return mapDtoToProduct(res.data);
  },
  
  delete: async (id: string) => {
    await axios.delete(`${BASE}/${id}`);
  }
};
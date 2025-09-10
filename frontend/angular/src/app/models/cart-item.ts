export interface CartItem {
  productID: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  category: string;
  imgUrl?: string; 
}
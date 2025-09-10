export interface Product {
  id: string;           // ProductId
  name: string;         // Productname
  category: string;     // Category
  unitPrice: number;    // Unitprice
  quantity: number;     // QuantityInStock
  imgUrl?: string;       // imgUrl
}
export interface NewOrderItemRequest {
    productID: string;
    productName: string;
    unitPrice: number;
    quantity: number;
    category:string;
    imgUrl?: string;
}

export interface Account {
  accountId: string;
  cashBalance: number;
}

export interface StockQuote {
  symbol: string;
  name: string;
  price: number;
}

export interface OrderRequest {
  accountId: string;
  symbol: string;
  quantity: number;
}

export interface OrderValidation {
  isValid: boolean;
  pricePerShare: number;
  totalAmount: number;
  errorMessage: string | null;
}

export interface Order {
  orderId: string;
  symbol: string;
  quantity: number;
  pricePerShare: number;
  totalAmount: number;
  executedAt: string;
  remainingCashBalance: number;
}

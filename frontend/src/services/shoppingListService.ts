import { API_BASE_URL } from "../config";

export interface ShoppingListItem {
  id: number;
  quantity: number;
  productId: number;
  productName: string | null;
  productCode: string | null;
  imageUrl: string | null;
  source: string;
  brands: string | null;
}

export interface ShoppingList {
  id: number;
  userId: string;
  items: ShoppingListItem[];
}

export interface AddProductRequest {
  productId: number;
  quantity: number;
}

export interface UpdateItemRequest {
  quantity: number;
}

export interface AddProductResult {
  success: boolean;
  message: string;
  item: ShoppingListItem | null;
}

export async function getShoppingList(): Promise<ShoppingList> {
  const response = await fetch(`${API_BASE_URL}/shopping-list`, {
    method: "GET",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch shopping list: ${response.statusText}`);
  }

  return response.json();
}

export async function addItemToShoppingList(
  productId: number,
  quantity: number
): Promise<AddProductResult> {
  const response = await fetch(`${API_BASE_URL}/shopping-list/items`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ productId, quantity } as AddProductRequest),
  });

  if (!response.ok) {
    throw new Error(`Failed to add item: ${response.statusText}`);
  }

  return response.json();
}

export async function updateShoppingListItem(
  itemId: number,
  quantity: number
): Promise<AddProductResult> {
  const response = await fetch(
    `${API_BASE_URL}/shopping-list/items/${itemId}`,
    {
      method: "PUT",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ quantity } as UpdateItemRequest),
    }
  );

  if (!response.ok) {
    throw new Error(`Failed to update item: ${response.statusText}`);
  }

  return response.json();
}

export async function removeItemFromShoppingList(
  itemId: number
): Promise<void> {
  const response = await fetch(
    `${API_BASE_URL}/shopping-list/items/${itemId}`,
    {
      method: "DELETE",
      credentials: "include",
    }
  );

  if (!response.ok) {
    throw new Error(`Failed to remove item: ${response.statusText}`);
  }
}

export async function clearShoppingList(): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/shopping-list`, {
    method: "DELETE",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error(`Failed to clear shopping list: ${response.statusText}`);
  }
}

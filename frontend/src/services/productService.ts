import { API_BASE_URL, API_ENDPOINTS } from "../config";

export type ProductSource = "OpenFoodFacts" | "AI" | "User";

export type ProductDto = {
  productId?: number | string;
  id?: number | string;
  name?: string;
  brand?: string;
  category?: string;
  source?: ProductSource;
};

export type ProductSearchResponse = {
  products: ProductDto[];
  totalCount: number;
  hasMore: boolean;
};

export const searchProducts = async (
  query: string,
  limit: number = 10
): Promise<ProductDto[]> => {
  try {
    if (!query || query.trim().length === 0) {
      return [];
    }

    const params = new URLSearchParams({
      query: query.trim(),
      limit: limit.toString(),
      offset: "0",
    });

    const response = await fetch(
      `${API_BASE_URL}${API_ENDPOINTS.PRODUCTS.SEARCH}?${params}`,
      {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      }
    );

    if (!response.ok) {
      console.error("Failed to search products:", response.statusText);
      return [];
    }

    const data: ProductSearchResponse = await response.json();
    return data.products || [];
  } catch (error) {
    console.error("Error searching products:", error);
    return [];
  }
};

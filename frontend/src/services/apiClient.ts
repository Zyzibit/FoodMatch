// Tokens are now managed via httpOnly cookies
// Backend automatically includes Authorization header from cookies
export const getAuthHeaders = (): HeadersInit => {
  return {
    "Content-Type": "application/json",
  };
};

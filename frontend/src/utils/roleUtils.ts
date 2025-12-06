import type { UserInfo } from "../types/auth";

export const isAdmin = (user: UserInfo | null): boolean => {
  if (!user || !user.roles) return false;
  return user.roles.includes("Admin") || user.roles.includes("Administrator");
};

export const hasRole = (user: UserInfo | null, role: string): boolean => {
  if (!user || !user.roles) return false;
  return user.roles.includes(role);
};

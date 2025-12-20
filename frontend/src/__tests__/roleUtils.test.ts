import { describe, expect, it } from "vitest";
import { hasRole, isAdmin } from "../utils/roleUtils";

const baseUser = { username: "u", roles: ["User"] };

describe("role utils", () => {
  it("detects admin roles", () => {
    expect(isAdmin({ ...baseUser, roles: ["Admin"] } as never)).toBe(true);
    expect(isAdmin({ ...baseUser, roles: ["Administrator"] } as never)).toBe(
      true
    );
    expect(isAdmin({ ...baseUser, roles: ["User"] } as never)).toBe(false);
  });

  it("returns false when user or roles missing", () => {
    expect(hasRole(null, "Admin")).toBe(false);
    expect(hasRole({ ...baseUser, roles: undefined } as never, "User")).toBe(
      false
    );
  });
});

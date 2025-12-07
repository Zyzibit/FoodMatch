import {
  Autocomplete,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  IconButton,
  Paper,
  Stack,
  TextField,
  Typography,
  Alert,
} from "@mui/material";
import { Add, Clear, Delete } from "@mui/icons-material";
import { useCallback, useEffect, useMemo, useState } from "react";
import { searchProducts, type ProductDto } from "../services/productService";
import {
  getShoppingList,
  addItemToShoppingList,
  removeItemFromShoppingList,
  clearShoppingList,
  type ShoppingListItem,
} from "../services/shoppingListService";

export default function ShoppingListPage() {
  const [search, setSearch] = useState("");
  const [selectedProduct, setSelectedProduct] = useState<ProductDto | null>(
    null
  );
  const [customProductName, setCustomProductName] = useState("");
  const [quantity, setQuantity] = useState("");
  const [items, setItems] = useState<ShoppingListItem[]>([]);
  const [checkedItems, setCheckedItems] = useState<Set<number>>(new Set());
  const [localItems, setLocalItems] = useState<
    Array<{
      id: string;
      productName: string;
      quantity: number;
    }>
  >([]);
  const [checkedLocalItems, setCheckedLocalItems] = useState<Set<string>>(
    new Set()
  );
  const [productSuggestions, setProductSuggestions] = useState<ProductDto[]>(
    []
  );
  const [isLoadingProducts, setIsLoadingProducts] = useState(false);
  const [isLoadingList, setIsLoadingList] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch shopping list on mount
  useEffect(() => {
    const fetchList = async () => {
      setIsLoadingList(true);
      setError(null);
      try {
        const list = await getShoppingList();
        setItems(list.items);
      } catch (err) {
        console.error("Error fetching shopping list:", err);
        setError("Nie udało się wczytać listy zakupów");
      } finally {
        setIsLoadingList(false);
      }
    };

    fetchList();
  }, []);

  const handleSearchProducts = useCallback(async (query: string) => {
    if (!query || query.trim().length < 2) {
      setProductSuggestions([]);
      return;
    }

    setIsLoadingProducts(true);
    try {
      const results = await searchProducts(query, 10);
      setProductSuggestions(results);
    } catch (error) {
      console.error("Error searching products:", error);
      setProductSuggestions([]);
    } finally {
      setIsLoadingProducts(false);
    }
  }, []);

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      handleSearchProducts(search);
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [search, handleSearchProducts]);

  const canAddItem =
    (selectedProduct !== null || customProductName.trim().length > 0) &&
    quantity.trim().length > 0;

  const handleAddItem = async () => {
    if (!canAddItem) return;

    setError(null);

    // If user selected from database
    if (selectedProduct) {
      try {
        const result = await addItemToShoppingList(
          selectedProduct.id,
          parseFloat(quantity)
        );
        if (result.success && result.item) {
          setItems((prev) => [...prev, result.item!]);
          setSearch("");
          setSelectedProduct(null);
          setCustomProductName("");
          setQuantity("");
        } else {
          setError(result.message || "Nie udało się dodać produktu");
        }
      } catch (err) {
        console.error("Error adding item:", err);
        setError("Nie udało się dodać produktu");
      }
    } else if (customProductName.trim()) {
      // If user entered custom name (not from database)
      setLocalItems((prev) => [
        ...prev,
        {
          id: crypto.randomUUID(),
          productName: customProductName.trim(),
          quantity: parseFloat(quantity),
        },
      ]);
      setSearch("");
      setCustomProductName("");
      setQuantity("");
    }
  };

  const handleRemove = async (id: number) => {
    setError(null);
    try {
      await removeItemFromShoppingList(id);
      setItems((prev) => prev.filter((item) => item.id !== id));
    } catch (err) {
      console.error("Error removing item:", err);
      setError("Nie udało się usunąć produktu");
    }
  };

  const handleRemoveLocal = (id: string) => {
    setLocalItems((prev) => prev.filter((item) => item.id !== id));
    setCheckedLocalItems((prev) => {
      const newSet = new Set(prev);
      newSet.delete(id);
      return newSet;
    });
  };

  const handleToggle = (id: number) => {
    setCheckedItems((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(id)) {
        newSet.delete(id);
      } else {
        newSet.add(id);
      }
      return newSet;
    });
  };

  const handleToggleLocal = (id: string) => {
    setCheckedLocalItems((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(id)) {
        newSet.delete(id);
      } else {
        newSet.add(id);
      }
      return newSet;
    });
  };

  const handleClearChecked = () => {
    setCheckedItems(new Set());
    setCheckedLocalItems(new Set());
  };

  const handleClear = async () => {
    setError(null);
    try {
      await clearShoppingList();
      setItems([]);
      setLocalItems([]);
    } catch (err) {
      console.error("Error clearing list:", err);
      setError("Nie udało się wyczyścić listy");
    }
  };

  const renderedItems = useMemo(
    () => (
      <>
        {items.map((item) => {
          const isChecked = checkedItems.has(item.id);
          return (
            <Box
              key={`db-${item.id}`}
              sx={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                borderBottom: (theme) => `1px solid ${theme.palette.grey[200]}`,
                py: 1.5,
                px: 1,
                cursor: "pointer",
                "&:hover": {
                  bgcolor: "action.hover",
                },
              }}
            >
              <Box
                sx={{ display: "flex", alignItems: "center", flex: 1, gap: 1 }}
                onClick={() => handleToggle(item.id)}
              >
                <Checkbox
                  checked={isChecked}
                  onChange={() => handleToggle(item.id)}
                />
                <Box>
                  <Typography
                    variant="body1"
                    sx={{
                      textDecoration: isChecked ? "line-through" : "none",
                      color: isChecked ? "text.disabled" : "text.primary",
                    }}
                  >
                    {item.productName}
                  </Typography>
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{
                      textDecoration: isChecked ? "line-through" : "none",
                    }}
                  >
                    {item.quantity} g
                  </Typography>
                  {item.brands && (
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      sx={{
                        textDecoration: isChecked ? "line-through" : "none",
                      }}
                    >
                      {item.brands}
                    </Typography>
                  )}
                </Box>
              </Box>
              <IconButton onClick={() => handleRemove(item.id)}>
                <Delete />
              </IconButton>
            </Box>
          );
        })}
        {localItems.map((item) => {
          const isChecked = checkedLocalItems.has(item.id);
          return (
            <Box
              key={`local-${item.id}`}
              sx={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                borderBottom: (theme) => `1px solid ${theme.palette.grey[200]}`,
                py: 1.5,
                px: 1,
                bgcolor: "action.hover",
                cursor: "pointer",
                "&:hover": {
                  bgcolor: "action.selected",
                },
              }}
            >
              <Box
                sx={{ display: "flex", alignItems: "center", flex: 1, gap: 1 }}
                onClick={() => handleToggleLocal(item.id)}
              >
                <Checkbox
                  checked={isChecked}
                  onChange={() => handleToggleLocal(item.id)}
                />
                <Box>
                  <Typography
                    variant="body1"
                    sx={{
                      textDecoration: isChecked ? "line-through" : "none",
                      color: isChecked ? "text.disabled" : "text.primary",
                    }}
                  >
                    {item.productName}
                  </Typography>
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    sx={{
                      textDecoration: isChecked ? "line-through" : "none",
                    }}
                  >
                    {item.quantity} g
                  </Typography>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    fontStyle="italic"
                    sx={{
                      textDecoration: isChecked ? "line-through" : "none",
                    }}
                  >
                    (własny produkt)
                  </Typography>
                </Box>
              </Box>
              <IconButton onClick={() => handleRemoveLocal(item.id)}>
                <Delete />
              </IconButton>
            </Box>
          );
        })}
      </>
    ),
    [items, localItems, checkedItems, checkedLocalItems]
  );

  return (
    <Box
      sx={{
        width: "100%",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
      }}
    >
      <Typography
        variant="h4"
        sx={{ fontWeight: 800, mb: 3, width: "100%", maxWidth: 900 }}
      >
        Do kupienia
      </Typography>

      {error && (
        <Alert severity="error" sx={{ width: "100%", maxWidth: 900, mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper sx={{ width: "100%", maxWidth: 900, p: 3 }}>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} mb={3}>
          <Autocomplete
            freeSolo
            options={productSuggestions}
            getOptionLabel={(option) =>
              typeof option === "string" ? option : (option.name ?? "")
            }
            value={selectedProduct}
            inputValue={search}
            onChange={(_, value) => {
              if (typeof value === "string") {
                setSelectedProduct(null);
                setCustomProductName(value);
              } else {
                setSelectedProduct(value);
                setCustomProductName("");
              }
            }}
            onInputChange={(_, value) => {
              setSearch(value);
              setCustomProductName(value);
            }}
            loading={isLoadingProducts}
            renderInput={(params) => (
              <TextField {...params} label="Produkt" placeholder="np. banany" />
            )}
            renderOption={(props, option) => {
              if (typeof option === "string") return null;
              return (
                <li {...props}>
                  <Box>
                    <Typography variant="body2">{option.name}</Typography>
                    {option.brand && (
                      <Typography variant="caption" color="text.secondary">
                        {option.brand}
                      </Typography>
                    )}
                  </Box>
                </li>
              );
            }}
            sx={{ flex: 2 }}
          />

          <TextField
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
            label="Ilość (g)"
            placeholder="500"
            type="number"
            inputProps={{ min: 0.01, step: 0.01 }}
            sx={{ width: 160 }}
          />

          <Button
            variant="contained"
            startIcon={<Add />}
            disabled={!canAddItem}
            onClick={handleAddItem}
            sx={{ whiteSpace: "nowrap", textTransform: "none" }}
          >
            Dodaj
          </Button>
        </Stack>

        <Paper variant="outlined" sx={{ maxHeight: 420, overflowY: "auto" }}>
          {isLoadingList ? (
            <Box sx={{ p: 3, textAlign: "center" }}>
              <CircularProgress />
            </Box>
          ) : items.length === 0 && localItems.length === 0 ? (
            <Box sx={{ p: 3, textAlign: "center" }}>
              <Typography variant="body2" color="text.secondary">
                Brak produktów na liście. Dodaj coś powyżej.
              </Typography>
            </Box>
          ) : (
            renderedItems
          )}
        </Paper>

        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          justifyContent="flex-end"
          mt={3}
        >
          <Button
            variant="outlined"
            startIcon={<Clear />}
            onClick={handleClearChecked}
            sx={{ textTransform: "none" }}
            disabled={checkedItems.size === 0 && checkedLocalItems.size === 0}
          >
            Odznacz zaznaczone
          </Button>
          <Button
            variant="outlined"
            startIcon={<Clear />}
            color="error"
            onClick={handleClear}
            sx={{ textTransform: "none" }}
          >
            Wyczyść listę zakupów
          </Button>
        </Stack>
      </Paper>
    </Box>
  );
}

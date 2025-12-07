import {
  Paper,
  Stack,
  Typography,
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
} from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import { useEffect, useState } from "react";
import {
  getAllUsers,
  deleteUser,
  updateUser,
  type AdminUserInfo,
  type UpdateUserRequest,
} from "../services/adminService";

export default function AdminUsersPage() {
  const [users, setUsers] = useState<AdminUserInfo[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [editForm, setEditForm] = useState<UpdateUserRequest>({
    name: "",
    email: "",
  });

  const fetchUsers = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getAllUsers(100, 0);
      // Filtruj admina - nie pokazuj go na liście
      const filteredUsers = result.users.filter(
        (user) => user.username !== "admin111222"
      );
      setUsers(filteredUsers);
    } catch (err) {
      console.error("Error fetching users:", err);
      setError(
        err instanceof Error ? err.message : "Nie udało się pobrać użytkowników"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const handleDeleteClick = (userId: string) => {
    setSelectedUserId(userId);
    setDeleteDialogOpen(true);
  };

  const handleEditClick = (user: AdminUserInfo) => {
    setSelectedUserId(user.id);
    setEditForm({
      name: user.username,
      email: user.email,
    });
    setEditDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!selectedUserId) return;

    try {
      await deleteUser(selectedUserId);
      setUsers(users.filter((u) => u.id !== selectedUserId));
      setDeleteDialogOpen(false);
      setSelectedUserId(null);
    } catch (err) {
      console.error("Error deleting user:", err);
      alert(
        err instanceof Error ? err.message : "Nie udało się usunąć użytkownika"
      );
    }
  };

  const handleEditConfirm = async () => {
    if (!selectedUserId) return;

    try {
      await updateUser(selectedUserId, editForm);
      // Odśwież listę użytkowników
      await fetchUsers();
      setEditDialogOpen(false);
      setSelectedUserId(null);
      setEditForm({ name: "", email: "" });
    } catch (err) {
      console.error("Error updating user:", err);
      alert(
        err instanceof Error
          ? err.message
          : "Nie udało się zaktualizować użytkownika"
      );
    }
  };

  return (
    <Paper
      elevation={1}
      sx={{ p: 3, width: "100%", maxWidth: 1400, mx: "auto" }}
    >
      <Stack spacing={3}>
        <Typography variant="h5" fontWeight={800}>
          Zarządzanie użytkownikami
        </Typography>

        {loading && (
          <Stack alignItems="center" py={4}>
            <CircularProgress />
          </Stack>
        )}

        {error && <Alert severity="error">{error}</Alert>}

        {!loading && !error && (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>
                    <strong>Username</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Email</strong>
                  </TableCell>
                  <TableCell>
                    <strong>Data utworzenia</strong>
                  </TableCell>
                  <TableCell align="right">
                    <strong>Akcje</strong>
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {users.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>{user.username}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>
                      {new Date(user.createdAt).toLocaleDateString("pl-PL")}
                    </TableCell>
                    <TableCell align="right">
                      <Stack
                        direction="row"
                        spacing={1}
                        justifyContent="flex-end"
                      >
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => handleEditClick(user)}
                          title="Edytuj użytkownika"
                        >
                          <Edit fontSize="small" />
                        </IconButton>
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteClick(user.id)}
                          title="Usuń użytkownika"
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Stack>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>Potwierdź usunięcie</DialogTitle>
        <DialogContent>
          <Typography>
            Czy na pewno chcesz usunąć tego użytkownika? Tej operacji nie można
            cofnąć.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Anuluj</Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
          >
            Usuń
          </Button>
        </DialogActions>
      </Dialog>

      {/* Edit User Dialog */}
      <Dialog
        open={editDialogOpen}
        onClose={() => setEditDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Edytuj użytkownika</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Nazwa użytkownika"
              fullWidth
              value={editForm.name}
              onChange={(e) =>
                setEditForm({ ...editForm, name: e.target.value })
              }
            />
            <TextField
              label="Email"
              type="email"
              fullWidth
              value={editForm.email}
              onChange={(e) =>
                setEditForm({ ...editForm, email: e.target.value })
              }
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Anuluj</Button>
          <Button
            onClick={handleEditConfirm}
            color="primary"
            variant="contained"
          >
            Zapisz
          </Button>
        </DialogActions>
      </Dialog>
    </Paper>
  );
}

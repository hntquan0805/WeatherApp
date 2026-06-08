import { createSlice, createAsyncThunk, type PayloadAction } from "@reduxjs/toolkit";
import type { LoginDto, RegisterDto, UserDto } from "../../types";
import { authApi } from "../../api/authApi";

interface AuthState {
  user: UserDto | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  user:            null,
  token:           null,
  isAuthenticated: false,
  isLoading:       false,
  error:           null,
};

// ── Async Thunks ──────────────────────────────────────

export const loginThunk = createAsyncThunk(
  "auth/login",
  async (dto: LoginDto, { rejectWithValue }) => {
    try {
      const { data: res } = await authApi.login(dto);
      if (!res.success) return rejectWithValue(res.message);
      return res.data;
    } catch (err: any) {
      return rejectWithValue(
        err.response?.data?.message ?? "Đăng nhập thất bại."
      );
    }
  }
);

export const registerThunk = createAsyncThunk(
  "auth/register",
  async (dto: RegisterDto, { rejectWithValue }) => {
    try {
      const { data: res } = await authApi.register(dto);
      if (!res.success) return rejectWithValue(res.message);
      return res.data;
    } catch (err: any) {
      return rejectWithValue(
        err.response?.data?.message ?? "Đăng ký thất bại."
      );
    }
  }
);

export const getProfileThunk = createAsyncThunk(
  "auth/getProfile",
  async (_, { rejectWithValue }) => {
    try {
      const { data: res } = await authApi.getProfile();
      if (!res.success) return rejectWithValue(res.message);
      return res.data;
    } catch (err: any) {
      return rejectWithValue(
        err.response?.data?.message ?? "Lấy profile thất bại."
      );
    }
  }
);

// ── Slice ─────────────────────────────────────────────

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    // Restore session từ localStorage khi app khởi động
    restoreSession(state) {
      const token = localStorage.getItem("token");
      const user  = localStorage.getItem("user");

      if (token && user) {
        try {
          const payload   = JSON.parse(atob(token.split(".")[1]));
          const isExpired = payload.exp * 1000 < Date.now();

          if (!isExpired) {
            state.token           = token;
            state.user            = JSON.parse(user);
            state.isAuthenticated = true;
          } else {
            localStorage.removeItem("token");
            localStorage.removeItem("user");
          }
        } catch {
          localStorage.removeItem("token");
          localStorage.removeItem("user");
        }
      }
    },

    logout(state) {
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      state.token           = null;
      state.user            = null;
      state.isAuthenticated = false;
      state.error           = null;
    },

    clearError(state) {
      state.error = null;
    },

    updateUser(state, action: PayloadAction<UserDto>) {
      state.user = action.payload;
      localStorage.setItem("user", JSON.stringify(action.payload));
    },
  },
  extraReducers: (builder) => {
    // ── Login ──────────────────────────────────────
    builder
      .addCase(loginThunk.pending, (state) => {
        state.isLoading = true;
        state.error     = null;
      })
      .addCase(loginThunk.fulfilled, (state, action) => {
        state.isLoading       = false;
        state.token           = action.payload.token;
        state.user            = action.payload.user;
        state.isAuthenticated = true;
        localStorage.setItem("token", action.payload.token);
        localStorage.setItem("user", JSON.stringify(action.payload.user));
      })
      .addCase(loginThunk.rejected, (state, action) => {
        state.isLoading = false;
        state.error     = action.payload as string;
      });

    // ── Register ───────────────────────────────────
    builder
      .addCase(registerThunk.pending, (state) => {
        state.isLoading = true;
        state.error     = null;
      })
      .addCase(registerThunk.fulfilled, (state, action) => {
        state.isLoading       = false;
        state.token           = action.payload.token;
        state.user            = action.payload.user;
        state.isAuthenticated = true;
        localStorage.setItem("token", action.payload.token);
        localStorage.setItem("user", JSON.stringify(action.payload.user));
      })
      .addCase(registerThunk.rejected, (state, action) => {
        state.isLoading = false;
        state.error     = action.payload as string;
      });

    // ── Get Profile ────────────────────────────────
    builder
      .addCase(getProfileThunk.fulfilled, (state, action) => {
        state.user = action.payload;
        localStorage.setItem("user", JSON.stringify(action.payload));
      });
  },
});

export const { restoreSession, logout, clearError, updateUser } = authSlice.actions;
export default authSlice.reducer;
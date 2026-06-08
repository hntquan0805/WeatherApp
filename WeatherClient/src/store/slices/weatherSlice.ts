import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import type { WeatherDto, WeatherLogDto } from "../../types";
import { weatherApi } from "../../api/weatherApi";

interface WeatherState {
  current:   WeatherDto | null;
  history:   WeatherLogDto[];
  isLoading: boolean;
  error:     string | null;
}

const initialState: WeatherState = {
  current:   null,
  history:   [],
  isLoading: false,
  error:     null,
};

// ── Async Thunks ──────────────────────────────────────

export const fetchWeatherThunk = createAsyncThunk(
  "weather/fetch",
  async (
    { city, units = "metric" }: { city: string; units?: string },
    { rejectWithValue }
  ) => {
    try {
      const { data: res } = await weatherApi.getWeather(city, units);
      if (!res.success) return rejectWithValue(res.message);
      return res.data;
    } catch (err: any) {
      return rejectWithValue(
        err.response?.data?.message ?? `Không tìm thấy thành phố "${city}".`
      );
    }
  }
);

export const fetchHistoryThunk = createAsyncThunk(
  "weather/fetchHistory",
  async (_, { rejectWithValue }) => {
    try {
      const { data: res } = await weatherApi.getHistory();
      if (!res.success) return rejectWithValue(res.message);
      return res.data;
    } catch (err: any) {
      return rejectWithValue(
        err.response?.data?.message ?? "Lấy lịch sử thất bại."
      );
    }
  }
);

export const deleteHistoryThunk = createAsyncThunk(
  "weather/deleteHistory",
  async (_, { rejectWithValue }) => {
    try {
      const { data: res } = await weatherApi.deleteHistory();
      if (!res.success) return rejectWithValue(res.message);
      return res.data;
    } catch (err: any) {
      return rejectWithValue(
        err.response?.data?.message ?? "Xoá lịch sử thất bại."
      );
    }
  }
);

// ── Slice ─────────────────────────────────────────────

const weatherSlice = createSlice({
  name: "weather",
  initialState,
  reducers: {
    clearWeather(state) {
      state.current = null;
      state.error   = null;
    },
    clearError(state) {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // ── Fetch Weather ──────────────────────────────
    builder
      .addCase(fetchWeatherThunk.pending, (state) => {
        state.isLoading = true;
        state.error     = null;
      })
      .addCase(fetchWeatherThunk.fulfilled, (state, action) => {
        state.isLoading = false;
        state.current   = action.payload;
      })
      .addCase(fetchWeatherThunk.rejected, (state, action) => {
        state.isLoading = false;
        state.current   = null;
        state.error     = action.payload as string;
      });

    // ── Fetch History ──────────────────────────────
    builder
      .addCase(fetchHistoryThunk.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(fetchHistoryThunk.fulfilled, (state, action) => {
        state.isLoading = false;
        state.history   = action.payload;
      })
      .addCase(fetchHistoryThunk.rejected, (state, action) => {
        state.isLoading = false;
        state.error     = action.payload as string;
      });

    // ── Delete History ─────────────────────────────
    builder
      .addCase(deleteHistoryThunk.fulfilled, (state) => {
        state.history = [];
      })
      .addCase(deleteHistoryThunk.rejected, (state, action) => {
        state.error = action.payload as string;
      });
  },
});

export const { clearWeather, clearError } = weatherSlice.actions;
export default weatherSlice.reducer;
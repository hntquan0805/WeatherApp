import { useCallback } from "react";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import {
  fetchWeatherThunk,
  fetchHistoryThunk,
  deleteHistoryThunk,
  clearWeather,
  clearError,
} from "../store/slices/weatherSlice";

export function useWeather() {
  const dispatch = useAppDispatch();
  const { current, history, isLoading, error } =
    useAppSelector((s) => s.weather);

  const search = useCallback(
    (city: string, units = "metric") =>
      dispatch(fetchWeatherThunk({ city, units })),
    [dispatch]
  );

  const loadHistory = useCallback(
    () => dispatch(fetchHistoryThunk()),
    [dispatch]
  );

  const deleteHistory = useCallback(
    () => dispatch(deleteHistoryThunk()),
    [dispatch]
  );

  const reset = useCallback(() => {
    dispatch(clearWeather());
    dispatch(clearError());
  }, [dispatch]);

  return {
    current,
    history,
    isLoading,
    error,
    search,
    loadHistory,
    deleteHistory,
    reset,
  };
}
import axiosInstance from "./axiosInstance";
import type {
  WeatherDto,
  WeatherLogDto,
  WeatherRequestDto,
  ApiResponseDto,
} from "../types";

export const weatherApi = {
  getWeather: (city: string, units = "metric") =>
    axiosInstance.get<ApiResponseDto<WeatherDto>>(
      `/weather?city=${encodeURIComponent(city)}&units=${units}`
    ),

  searchWeather: (dto: WeatherRequestDto) =>
    axiosInstance.post<ApiResponseDto<WeatherDto>>("/weather/search", dto),

  getHistory: () =>
    axiosInstance.get<ApiResponseDto<WeatherLogDto[]>>("/weather/history"),

  getRecentHistory: (count = 10) =>
    axiosInstance.get<ApiResponseDto<WeatherLogDto[]>>(
      `/weather/history/recent?count=${count}`
    ),

  deleteHistory: () =>
    axiosInstance.delete<ApiResponseDto<null>>("/weather/history"),
};
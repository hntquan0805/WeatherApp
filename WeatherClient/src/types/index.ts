// ── Auth ──────────────────────────────────────────────
export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface UserDto {
  id: number;
  username: string;
  email: string;
  role: string;
  createdAt: string;
}

export interface AuthResponseDto {
  token: string;
  expiresAt: string;
  user: UserDto;
}

export interface UpdateUserDto {
  username?: string;
  email?: string;
  currentPassword?: string;
  newPassword?: string;
}

// ── Weather ───────────────────────────────────────────
export interface WeatherDto {
  city: string;
  country: string;
  temperature: number;
  feelsLike: number;
  tempMin: number;
  tempMax: number;
  humidity: number;
  windSpeed: number;
  pressure: number;
  visibility: number;
  description: string;
  icon: string;
  iconUrl: string;
  sunrise: string;
  sunset: string;
  searchedAt: string;
  unit: string;
}

export interface WeatherLogDto {
  id: number;
  city: string;
  country: string;
  temperature: number;
  humidity: number;
  description: string;
  icon: string;
  iconUrl: string;
  searchedAt: string;
}

export interface WeatherRequestDto {
  city: string;
  units: "metric" | "imperial" | "standard";
}

// ── API Response wrapper ──────────────────────────────
export interface ApiResponseDto<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}
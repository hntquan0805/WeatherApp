import axiosInstance from "./axiosInstance";
import type {
  LoginDto,
  RegisterDto,
  UpdateUserDto,
  AuthResponseDto,
  UserDto,
  ApiResponseDto,
} from "../types";

export const authApi = {
  login: (dto: LoginDto) =>
    axiosInstance.post<ApiResponseDto<AuthResponseDto>>("/auth/login", dto),

  register: (dto: RegisterDto) =>
    axiosInstance.post<ApiResponseDto<AuthResponseDto>>("/auth/register", dto),

  getProfile: () =>
    axiosInstance.get<ApiResponseDto<UserDto>>("/auth/profile"),

  updateProfile: (dto: UpdateUserDto) =>
    axiosInstance.put<ApiResponseDto<UserDto>>("/auth/profile", dto),
};
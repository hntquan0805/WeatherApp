import { useDispatch, useSelector } from "react-redux";
import type { RootState, AppDispatch } from "./index";

// Dùng 2 hooks này thay cho useDispatch/useSelector gốc
// để có đầy đủ TypeScript type support
export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector = <T>(selector: (state: RootState) => T) =>
  useSelector(selector);
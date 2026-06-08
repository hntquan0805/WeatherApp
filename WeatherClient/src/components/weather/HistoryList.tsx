import { Trash2, Clock, Droplets } from "lucide-react";
import type { WeatherLogDto } from "../../types";

interface Props {
  history:  WeatherLogDto[];
  onSelect: (city: string) => void;
  onClear:  () => void;
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleString("vi-VN", {
    day:    "2-digit",
    month:  "2-digit",
    hour:   "2-digit",
    minute: "2-digit",
  });
}

export default function HistoryList({ history, onSelect, onClear }: Props) {
  if (history.length === 0) {
    return (
      <div className="text-center py-10 text-gray-400">
        <Clock size={32} className="mx-auto mb-2 opacity-40" />
        <p className="text-sm">Chưa có lịch sử tìm kiếm</p>
      </div>
    );
  }

  return (
    <div>
      {/* Header — ngoài khối, không có card wrapper */}
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-medium text-gray-700">
          Lịch sử tìm kiếm
          <span className="ml-2 text-xs text-gray-400">({history.length} kết quả)</span>
        </h3>
        <button
          onClick={onClear}
          className="flex items-center gap-1 text-xs text-red-400 hover:text-red-600 transition-colors"
        >
          <Trash2 size={12} />
          Xoá tất cả
        </button>
      </div>

      {/* Grid 3 cards / hàng */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        {history.map((log) => (
          <button
            key={log.id}
            onClick={() => onSelect(log.city)}
            className="flex flex-col rounded-2xl p-4 text-left group transition-all
                       hover:shadow-md hover:brightness-95"
            style={{ background: "rgba(184, 223, 245, 0.72)" }}
          >
            {/* Top: icon + temp */}
            <div className="flex items-center justify-between mb-3">
              <img
                src={log.iconUrl}
                alt={log.description}
                className="w-12 h-12"
              />
              <span className="text-2xl font-bold text-gray-800">
                {Math.round(log.temperature)}°C
              </span>
            </div>

            {/* City */}
            <p className="text-sm font-semibold text-gray-800 truncate leading-tight">
              {log.city}, {log.country}
            </p>
            <p className="text-xs text-gray-500 capitalize truncate mt-0.5 mb-3">
              {log.description}
            </p>

            {/* Stats */}
            <div className="flex items-center gap-3 mt-auto">
              <span className="flex items-center gap-1 text-xs text-gray-500">
                <Droplets size={11} className="text-blue-500" />
                {log.humidity}%
              </span>
            </div>

            {/* Date footer */}
            <p className="text-xs text-gray-400 mt-2 pt-2 border-t border-white/40">
              {formatDate(log.searchedAt)}
            </p>
          </button>
        ))}
      </div>
    </div>
  );
}
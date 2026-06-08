import { Wind, Droplets, Eye, Gauge, Sunrise, Sunset } from "lucide-react";
import type { WeatherDto } from "../../types";

interface Props {
  weather: WeatherDto;
}

function StatItem({
  icon, label, value,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
}) {
  return (
    <div className="flex items-center gap-3 rounded-xl px-4 py-3"
         style={{ background: "rgba(255,255,255,0.18)" }}>
      <div style={{ color: "rgba(30,60,90,0.7)" }}>{icon}</div>
      <div>
        <p className="text-xs" style={{ color: "rgba(30,60,90,0.55)" }}>{label}</p>
        <p className="text-sm font-semibold" style={{ color: "#1a3a52" }}>{value}</p>
      </div>
    </div>
  );
}

function formatTime(dateStr: string) {
  return new Date(dateStr).toLocaleTimeString("en-US", {
    hour: "2-digit", minute: "2-digit",
  });
}

export default function WeatherCard({ weather }: Props) {
  const tempUnit = weather.unit ?? "°C";
  const windUnit = weather.unit === "°F" ? "mph" : "m/s";

  return (
    <div className="weather-card-animated rounded-2xl p-6 shadow-lg">
      <style>{`
        @keyframes gradientShift {
          0%   { background-position: 0% 50%; }
          50%  { background-position: 100% 50%; }
          100% { background-position: 0% 50%; }
        }
        .weather-card-animated {
          background: linear-gradient(
            135deg,
            rgba(184, 223, 245, 0.95),
            rgba(147, 205, 237, 0.90),
            rgba(200, 232, 248, 0.95),
            rgba(120, 190, 230, 0.88),
            rgba(184, 223, 245, 0.92)
          );
          background-size: 300% 300%;
          animation: gradientShift 6s ease infinite;
        }
      `}</style>

      {/* Top row: city + icon */}
      <div className="flex items-start justify-between mb-4">
        <div>
          <h2 className="text-2xl font-bold" style={{ color: "#0f2d42" }}>
            {weather.city}, {weather.country}
          </h2>
          <p className="text-sm capitalize mt-0.5" style={{ color: "rgba(15,45,66,0.65)" }}>
            {weather.description}
          </p>
        </div>
        <img
          src={weather.iconUrl}
          alt={weather.description}
          className="w-16 h-16 -mt-1"
        />
      </div>

      {/* Temperature */}
      <div className="flex items-end gap-4 mb-6">
        <span className="text-6xl font-bold" style={{ color: "#0f2d42" }}>
          {Math.round(weather.temperature)}{tempUnit}
        </span>
        <div className="mb-2 text-sm" style={{ color: "rgba(15,45,66,0.65)" }}>
          <p>Feels like {Math.round(weather.feelsLike)}{tempUnit}</p>
          <p>↑{Math.round(weather.tempMax)}{tempUnit} ↓{Math.round(weather.tempMin)}{tempUnit}</p>
        </div>
      </div>

      {/* Stats grid */}
      <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
        <StatItem icon={<Droplets size={16} />} label="Humidity"   value={`${weather.humidity}%`} />
        <StatItem icon={<Wind     size={16} />} label="Wind"       value={`${weather.windSpeed} ${windUnit}`} />
        <StatItem icon={<Gauge    size={16} />} label="Pressure"   value={`${weather.pressure} hPa`} />
        <StatItem icon={<Eye      size={16} />} label="Visibility" value={`${(weather.visibility / 1000).toFixed(1)} km`} />
        <StatItem icon={<Sunrise  size={16} />} label="Sunrise"    value={formatTime(weather.sunrise)} />
        <StatItem icon={<Sunset   size={16} />} label="Sunset"     value={formatTime(weather.sunset)} />
      </div>
    </div>
  );
}
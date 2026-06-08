import { useState, useEffect, useRef } from "react";
import { Search, MapPin, Loader2, AlertCircle, X, Clock, Thermometer } from "lucide-react";
import { useWeather } from "../hooks/useWeather";
import { useAppSelector } from "../store/hooks";
import WeatherCard from "../components/weather/WeatherCard";
import HistoryList from "../components/weather/HistoryList";

const POPULAR_CITIES = [
  "Hanoi", "Ho Chi Minh City", "Da Nang", "Hue",
  "Can Tho", "Tokyo", "Singapore", "London", "New York", "Paris",
];

type Tab = "weather" | "history";

export default function WeatherPage() {
  const { isAuthenticated } = useAppSelector((s) => s.auth);
  const {
    current, history, isLoading, error,
    search, loadHistory, deleteHistory, reset,
  } = useWeather();

  const [query,       setQuery]       = useState("");
  const [units,       setUnits]       = useState<"metric" | "imperial">("metric");
  const [showSuggest, setShowSuggest] = useState(false);
  const [activeTab,   setActiveTab]   = useState<Tab>("weather");

  const inputRef   = useRef<HTMLInputElement>(null);
  const suggestRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (isAuthenticated) loadHistory();
  }, [isAuthenticated, loadHistory]);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (
        suggestRef.current &&
        !suggestRef.current.contains(e.target as Node) &&
        !inputRef.current?.contains(e.target as Node)
      ) {
        setShowSuggest(false);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  // Refresh history immediately after a successful search
  useEffect(() => {
    if (current && isAuthenticated) {
      loadHistory();
    }
  }, [current]);

  const handleSearch = (city: string) => {
    const trimmed = city.trim();
    if (!trimmed) return;
    setQuery(trimmed);
    setShowSuggest(false);
    search(trimmed, units);
    setActiveTab("weather");
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    handleSearch(query);
  };

  const handleClear = () => {
    reset();
    setQuery("");
    inputRef.current?.focus();
  };

  const filteredSuggestions = POPULAR_CITIES.filter((c) =>
    c.toLowerCase().includes(query.toLowerCase()) && query.length > 0
  );

  return (
    <div className="weather-page">

      {/* ── Page title ── */}
      <div className="weather-page__header">
        <h1 className="weather-page__title">Weather</h1>
        <p className="weather-page__subtitle">Search a city to see current weather</p>
      </div>

      {/* ── Tabs: centered ── */}
      <div className="weather-page__tabs">
        <button
          type="button"
          onClick={() => setActiveTab("weather")}
          className={`weather-page__tab ${activeTab === "weather" ? "weather-page__tab--active" : ""}`}
        >
          <Search size={14} />
          Weather
        </button>
        <button
          type="button"
          onClick={() => setActiveTab("history")}
          className={`weather-page__tab ${activeTab === "history" ? "weather-page__tab--active" : ""}`}
        >
          <Clock size={14} />
          History
          {history.length > 0 && (
            <span className="weather-page__tab-badge">{history.length}</span>
          )}
        </button>
      </div>

      {/* ── Search bar + toggle side by side (Weather tab only) ── */}
      {activeTab === "weather" && (
        <div className="weather-page__search-wrap">
          <form onSubmit={handleSubmit} className="weather-page__search-form">

            {/* Input */}
            <div className="weather-page__input-wrap">
              <button
                type="submit"
                disabled={isLoading || !query.trim()}
                className="weather-page__search-icon-btn"
                aria-label="Search"
              >
                {isLoading
                  ? <Loader2 size={15} className="animate-spin" />
                  : <Search size={15} />}
              </button>
              <input
                ref={inputRef}
                type="text"
                value={query}
                onChange={(e) => { setQuery(e.target.value); setShowSuggest(true); }}
                onFocus={(e) => { setShowSuggest(true); e.target.style.borderColor = "#4f6ef7"; }}
                onBlur={(e)  => { e.target.style.borderColor = "#d0d5e8"; }}
                placeholder="Enter city name... (e.g. Hanoi, Tokyo)"
                className="weather-page__input"
              />
              {query && (
                <button type="button" onClick={handleClear} className="weather-page__clear-btn">
                  <X size={15} />
                </button>
              )}
            </div>

            {/* Unit toggle — kế bên input */}
            <div className="weather-page__unit-toggle-wrap">
              <Thermometer size={13} style={{ color: "var(--color-text-muted)" }} />
              <span className="weather-page__unit-label">°C</span>
              <button
                type="button"
                onClick={() => setUnits((p) => p === "metric" ? "imperial" : "metric")}
                aria-label="Toggle Fahrenheit"
                className={`weather-page__unit-toggle ${units === "imperial" ? "weather-page__unit-toggle--on" : ""}`}
              >
                <span className="weather-page__unit-toggle-thumb" />
              </button>
              <span className="weather-page__unit-label">°F</span>
            </div>

          </form>

          {/* Suggestions dropdown */}
          {showSuggest && filteredSuggestions.length > 0 && (
            <div ref={suggestRef} className="weather-page__suggest">
              {filteredSuggestions.map((city) => (
                <button
                  key={city}
                  onMouseDown={() => handleSearch(city)}
                  className="weather-page__suggest-item"
                >
                  <MapPin size={13} className="weather-page__suggest-pin" />
                  {city}
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      {/* ── Weather result ── */}
      {activeTab === "weather" && (
        <div className="weather-page__result">
          {isLoading && (
            <div className="weather-page__state-center">
              <Loader2 size={36} className="animate-spin weather-page__state-spinner" />
              <p className="weather-page__state-text">Loading weather data...</p>
            </div>
          )}

          {error && !isLoading && (
            <div className="weather-page__error">
              <AlertCircle size={17} className="weather-page__error-icon" />
              <div>
                <p className="weather-page__error-title">No results found</p>
                <p className="weather-page__error-msg">{error}</p>
              </div>
            </div>
          )}

          {current && !isLoading && <WeatherCard weather={current} units={units} />}

          {!current && !isLoading && !error && (
            <div className="weather-page__state-center weather-page__state-center--empty">
              <Search size={48} className="weather-page__state-empty-icon" />
              <p className="weather-page__state-text">Enter a city name to get started</p>
            </div>
          )}
        </div>
      )}

      {/* ── History tab ── */}
      {activeTab === "history" && (
        <div>
          {isAuthenticated ? (
            <HistoryList
              history={history}
              onSelect={(city) => { handleSearch(city); }}
              onClear={deleteHistory}
            />
          ) : (
            <div className="weather-page__login-prompt">
              <p className="weather-page__login-prompt-title">Sign in to save history</p>
              <p className="weather-page__login-prompt-sub">Your searches will be saved and shown here</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
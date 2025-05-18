package main

import (
	"context"
	"errors"
	"io"
	"net/http"
	"strconv"
	"time"

	"github.com/go-chi/chi"
	"github.com/go-chi/chi/middleware"
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
)

var (
	requestsTotal = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "http_requests_total",
			Help: "Total number of HTTP requests",
		},
		[]string{"status", "method", "path"},
	)
	requestDuration = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name: "http_request_duration_seconds",
			Help: "Duration of HTTP requests",
		},
		[]string{"status", "method", "path"},
	)
)

func PiHandler(w http.ResponseWriter, r *http.Request) {
	ctx := r.Context()
	log := GetLogger(ctx)

	digits := 50
	if d := r.URL.Query().Get("digits"); d != "" {
		digits, _ = strconv.Atoi(d)
		if digits <= 0 {
			log.Warn("invalid digits", "value", d)
			http.Error(w, "digits param must be a number > 0", http.StatusBadRequest)
			return
		}
	}

	log.Info("calculating pi", "digits", digits)
	pi, err := CalculatePI(ctx, digits)

	if errors.Is(err, context.DeadlineExceeded) {
		log.Warn("timed out when calculating pi", "digits", digits)
		http.Error(w, "timeout :(", http.StatusRequestTimeout)
		return
	}

	if err != nil {
		log.Error("failed to calculate pi", "error", err)
		http.Error(w, "internal error :(", http.StatusInternalServerError)
		return
	}

	io.WriteString(w, pi)
}

func main() {
	prometheus.MustRegister(requestsTotal, requestDuration)

	r := chi.NewRouter()
	r.Use(TraceMiddleware)
	r.Use(LoggerMiddleware)
	r.Use(MetricsMiddleware)
	r.Use(middleware.Timeout(30 * time.Second))

	r.Handle("/metrics", promhttp.Handler())

	r.Get("/pi", PiHandler)

	http.ListenAndServe(":8080", r)
}

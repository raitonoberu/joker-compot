package main

import (
	"context"
	"log/slog"
	"net/http"
	"os"
	"strconv"
	"time"

	"github.com/go-chi/chi/middleware"
	"github.com/google/uuid"
)

func MetricsMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		ww := middleware.NewWrapResponseWriter(w, r.ProtoMajor)
		start := time.Now()

		next.ServeHTTP(ww, r)

		status := strconv.Itoa(ww.Status())
		requestsTotal.WithLabelValues(status, r.Method, r.URL.Path).Inc()
		requestDuration.WithLabelValues(status, r.Method, r.URL.Path).Observe(time.Since(start).Seconds())
	})
}

func LoggerMiddleware(next http.Handler) http.Handler {
	log := slog.New(slog.NewJSONHandler(os.Stdout, nil))
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		ww := middleware.NewWrapResponseWriter(w, r.ProtoMajor)
		start := time.Now()

		ctx := r.Context()
		log := log.With("trace", GetTrace(ctx))
		ctx = context.WithValue(ctx, loggerKey{}, log)

		log.Info("received request", "method", r.Method, "path", r.URL.Path, "query", r.URL.RawQuery)
		next.ServeHTTP(ww, r.WithContext(ctx))
		log.Info("sent response", "status", ww.Status(), "latency", time.Since(start).Seconds())
	})
}

func TraceMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		uuid := uuid.New()
		ctx := context.WithValue(r.Context(), traceKey{}, uuid)
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}

func GetLogger(ctx context.Context) *slog.Logger {
	return ctx.Value(loggerKey{}).(*slog.Logger)
}

func GetTrace(ctx context.Context) uuid.UUID {
	return ctx.Value(traceKey{}).(uuid.UUID)
}

type loggerKey struct{}

type traceKey struct{}

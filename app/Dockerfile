FROM golang:1.24-alpine AS builder
WORKDIR /build

COPY go.mod .
RUN go mod download
COPY . .
RUN CGO_ENABLED=0 GOOS=linux go build -o /main .

FROM alpine:3.21
COPY --from=builder main /bin/main
ENTRYPOINT ["/bin/main"]

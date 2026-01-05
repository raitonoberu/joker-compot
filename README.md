# Jocker Сompot microservices project

<img alt="Jocker Compot logo" src="compot.jpg" width="256px" height="256px" />

Этот репозиторий содержит проект курса по микросервисам с использованием Docker и Docker Compose. В проекте реализованы несколько микросервисов, с которыми взаимодействует шлюз через GRPC и преобразует ответы в формат JSON для клиента.

Реализовано:
- REST API шлюза (ASP.NET) с агрегацией данных
- Кеширование (Redis)
- Мониторинг (метрики, трейсы и логи через OTEL + Grafana)
- Retry/Fallback
- JWT авторизация с автоконфигурацией ключей через .well-known эндпоинт
- GRPC транспорт между шлюзом и микросервисами

## Запуск
```shell
docker compose up -d --build
```


После выполнения этой команды можно перейти в [Grafana](http://localhost:3000) (localhost:3000) для мониторинга или в [Scalar](http://localhost:8080) (localhost:8080) для выполнения запросов.

## Структура проекта
- `docker-compose.yml`: Файл конфигурации Docker Compose для запуска всех микросервисов и шлюза.
- `compot-gateway/`: Каталог с кодом шлюза, который обрабатывает GRPC
- `compot-orders-service/`: Каталог с кодом микросервиса заказов.
- `compot-users-service/`: Каталог с кодом микросервиса пользователей.
- `compot-products-service/`: Каталог с кодом микросервиса продуктов.
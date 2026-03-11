# Лабораторная работа №1 — «Кэширование»

## Описание

Микросервисный бэкенд для генерации кредитных заявок с кэшированием ответов в Redis. Оркестрация через .NET Aspire.

## Стек технологий

- .NET 8, ASP.NET Core Minimal API
- .NET Aspire (оркестрация, Service Defaults, Dashboard)
- Redis (кэширование через `IDistributedCache`)
- Redis Insight (веб-интерфейс для Redis)
- Bogus (генерация данных)
- Blazor WebAssembly (клиент)

## Что реализовано

### Сервис генерации данных
- Генератор `CreditApplicationGenerator` 
- `Faker<CreditApplication>` с бизнес-правилами:
  - Дата решения заполняется только для терминальных статусов
  - Одобренная сумма заполняется только при статусе «Одобрена» и не превышает запрашиваемую
  - Дата подачи — не более 2 лет назад от текущей даты
  - Процентная ставка — от 15.5%

### Кэширование
- `CreditApplicationService` с `IDistributedCache` (Redis)
- Ключ кэша: `credit-application:{id}`
- Время жизни кэша настраивается через `appsettings.json` (`CacheSettings:ExpirationMinutes`)
- Раздельная обработка ошибок: чтение из кэша, генерация, запись в кэш

### Оркестрация (.NET Aspire)
- `CreditApp.AppHost` управляет запуском всех компонентов
- Redis с веб-интерфейсом Redis Insight (`.WithRedisInsight()`)
- API ждёт готовности Redis (`.WaitFor(redis)`)
- Клиент ждёт готовности API (`.WaitFor(api)`)
- Aspire Dashboard для мониторинга логов, трейсов и метрик

## API

```
GET /credit-application?id={int}
```

Возвращает JSON с данными кредитной заявки. При повторном запросе с тем же `id` возвращает данные из кэша.


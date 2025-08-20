Салют, это резюме по заданию 4 на стажировке, сервис «Банковские счета»

Не успел за выходные доделать задачи: тесты, контролер с документация по событиям, да и самодельный outbox dispatcher и consumer вызывают сомнения, как и логгер.

Сам проект лежит на http://localhost:80 или просто http://localhost , автоматически редиректит на /swagger, keycloak на http://localhost:8080
hangfire на http://localhost/hangfire БД наружу на http://localhost:5433, Rabbit manager http://localhost:15672, сам rabbit http://localhost:5672/

В outbox и из него в rabbit события падают при создании счета или при любом переводе, входящие из шины можно создать rabbitmq manager - exchanges - publish
Routing key или client.blocked или client.unblocked, тело

{
  "eventId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "occurredAt": "2025-08-19T09:45:00Z",
  "clientId": "11111111-1111-1111-1111-111111111111",
  "meta": {
    "version": "v1",
    "source": "account-service",
    "correlationId": "11111111-1111-1111-1111-111111111111",
    "causationId": "22222222-2222-2222-2222-222222222222"
  }
}

Запускается или docker-compose up -d, или F5 в IDE, если выбрать docker-compose, билдится около минуты
Тесты запускаются dotnet test .\SimpleAccountService.Tests\ либо в контекстном меню

Аутентификация в swagger: пойти на эндпоинт Auth/GetToken, выполнить запрос, получить токен в формате "Bearer {token}", вставить токен в авторизацю сваггера(сверху справа) залогиниться и закрыть окно авторизации. Если неужно reseed базу, надо раскомментить //context.Database.EnsureDeleted(); в программ




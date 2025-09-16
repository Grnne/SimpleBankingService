Учебный проект микросервиса для работы с банковскими счетами

Архитектура: Practical Clean Architecture + Vertical Slices, заготовка под микросервис.

 
 Сам проект лежит на http://localhost:80 или просто http://localhost , автоматически редиректит на /swagger, keycloak на http://localhost:8080 hangfire на http://localhost/hangfire БД наружу на http://localhost:5433, Rabbit manager http://localhost:15672, сам rabbit http://localhost:5672/
 
 Запускается или docker-compose up -d, или F5 в IDE, если выбрать docker-compose, билдится около минуты Тесты запускаются dotnet test .\SimpleAccountService.Tests\ либо в контекстном меню

 Аутентификация в swagger: пойти на эндпоинт Auth/GetToken, выполнить запрос, получить токен в формате "Bearer {token}", вставить токен в авторизацю сваггера(сверху справа) залогиниться и закрыть окно авторизации. Если неужно reseed базу, надо раскомментить //context.Database.EnsureDeleted(); в программ
 

 TODO 
 Make from this init structure for Base Project Template. Pragmatic Clean Architecture + Vertical Slices, dummy for microservice, maybe redo for monolith
 
 Vue+Vite -- maybe TODO basic front with ai
  
 Dummy RabbitMq refactor and simplify inbox service
 
 Testing with testcontainers -- TODO adequate fixture, abuse chatgpt to make unit tests, make critical way tests, make some integrational tests

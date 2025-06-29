TelegramBot Документація
1. Налаштування та залежності
1.1. Системні вимоги
- .NET 6.0 SDK або вище
- Доступ до інтернету для Telegram Bot API та зовнішніх сервісів (Mindee, OpenAI)
1.2. Налаштування змінних середовища
•`TELEGRAM_BOT_TOKEN` – токен вашого Telegram-бота
•`MINDEE_API_KEY` – API-ключ для Mindee
•`OPENAI_API_KEY` – API-ключ для OpenAI
•*(опціонально)* `OPENAI_MODEL` – модель OpenAI (за замовчуванням `gpt-4o-mini`)
1.3. Інсталяція та запуск
```bashgit clone <repo-url>cd TelegramBotdotnet restoredotnet run --project TelegramBot```
У контейнерному середовищі:
```bashdocker build -t telegrambot .docker run -e TELEGRAM_BOT_TOKEN=... \           -e MINDEE_API_KEY=... \           -e OPENAI_API_KEY=... \           telegrambot```
2. Детальний опис роботи бота
2.1. Архітектура та компоненти
•PollingService – отримує оновлення від Telegram та делегує їх у UpdateHandler.
•UpdateHandler – визначає тип оновлення і передає у відповідний обробник.
•IUserSessionService (InMemoryUserSessionService) – зберігає стан сесії користувача (UserSession).
•Handlers: TextMessageHandler, PhotoMessageHandler, CallbackQueryHandler.
•Процесори документів: PassportProcessor, VehicleRegistrationProcessor.
•CompletionService – перевірка готовності документів та генерація полісу.
•OpenAiService – взаємодія з OpenAI для відповідей та генерації полісу.
•MindeeService – взаємодія з Mindee API для розпізнавання документів.
•TelegramFileService – завантаження фото, створення PDF, видалення тимчасових файлів.
•MenuService – відправка головного меню з інлайн-кнопками.
•ChatHistoryService – очищення історії чату користувача.
2.2. Стани сесії (DocumentState)
•AwaitingSelection – очікування вибору документа
•AwaitingPassportFront / AwaitingPassportBack – очікування фото з паспорта
•AwaitingPassportConfirmation – підтвердження даних паспорта
•AwaitingVehicleRegistrationFront / AwaitingVehicleRegistrationBack – очікування фото техпаспорта
•AwaitingVehicleRegistrationConfirmation – підтвердження даних техпаспорта
•AwaitingPriceConfirmation – підтвердження вартості полісу
•AwaitingQuestion – режим консультації з AI
3. Приклади сценаріїв взаємодії
3.1. Оформлення полісу
1.Користувач надсилає /start.
2.Бот надсилає меню з кнопками для завантаження документа.
3.Користувач завантажує фото паспорта, бот обробляє через Mindee та просить підтвердження.
4.Користувач підтверджує дані, бот зберігає їх та відправляє меню для техпаспорта.
5.Аналогічний процес для техпаспорта.
6.Після підтвердження обох документів бот пропонує підтвердити вартість (100 USD).
7.При підтвердженні бот генерує поліс через OpenAI та надсилає текст.
8.Бот інформує про набуття чинності полісу після оплати та пропонує /clear для очищення.
3.2. Консультація з AI
9.Користувач надсилає /conversation.
10.Бот переходить у режим консультації та чекає запитання.
11.Користувач пише питання, бот надсилає відповідь з OpenAI.
12.Для завершення відправляється /stop_chat, бот повертається у меню.
3.3. Очищення чату
13.Користувач надсилає /clear.
14.Бот видаляє накопичені повідомлення та пропонує /start для нової сесії.

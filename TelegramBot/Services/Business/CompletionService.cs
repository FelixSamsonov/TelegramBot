using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Business
{
    // Сервіс для перевірки завершення обробки документів та фіналізації полісу.
    public class CompletionService : ICompletionService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IOpenAiService _openAiService;
        private readonly IMenuService _menuService;

        public CompletionService(
            ITelegramBotClient botClient,
            IOpenAiService openAiService,
            IMenuService menuService)
        {
            _botClient = botClient;
            _openAiService = openAiService;
            _menuService = menuService;
        }
        /// <summary>
        /// Перевіряє, чи всі документи користувача завершені. Якщо так, запитує підтвердження вартості.
        /// </summary>
        /// <param name="session">Сесія користувача з інформацією про документи.</param>
        /// <param name="cancellationToken">Токен для скасування операції.</param>

        public async Task CheckCompletionAsync(UserSession session, CancellationToken cancellationToken)
        {
            if (session.IsComplete())
            {
                session.CurrentState = DocumentState.AwaitingPriceConfirmation;
                var kb = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("✅ Погоджуюсь", "confirm_price") },
                    new[] { InlineKeyboardButton.WithCallbackData("❌ Ні",          "decline_price") }
                });
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "🎉 Усі документи вірні! 💰 Вартість 100 USD. Погоджуєтесь?",
                    replyMarkup: kb,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _menuService.SendMainMenuAsync(session, cancellationToken);
            }
        }

        /// <summary>
        /// Завершує процес генерування полісу після підтвердження вартості.
        /// </summary>
        /// <param name="session">Сесія користувача з підтвердженими документами.</param>
        /// <param name="cancellationToken">Токен для скасування операції.</param>
        public async Task FinalizePolicyAsync(UserSession session, CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync(
                session.ChatId,
                "✅ Генерую страховий поліс… Це може зайняти хвилину.",
                cancellationToken: cancellationToken);

            var policy = await _openAiService
                              .GenerateInsurancePolicyTextAsync(session, cancellationToken);

            if (string.IsNullOrWhiteSpace(policy))
            {
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "❌ Помилка при генерації полісу. Спробуйте пізніше.",
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "🎉 Ваш поліс готовий!",
                    cancellationToken: cancellationToken);

                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    policy,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
            }

            await _botClient.SendTextMessageAsync(
                session.ChatId,
                "Поліс набуде чинності після оплати. Щоб очистити чат від фото документів натисніть /clear",
                cancellationToken: cancellationToken);
        }
    }
}

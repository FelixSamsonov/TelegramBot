using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations
{
    public class MenuService : IMenuService
    {
        private readonly ITelegramBotClient _botClient;

        public MenuService(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task SendMainMenuAsync(UserSession session, CancellationToken cancellationToken)
        {
            var ps = session.PassportData != null ? "✅" : "❌";
            var vs = session.VehicleRegistrationData != null ? "✅" : "❌";
            var kb = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData($"Завантажити паспорт {ps}",    "upload_passport") },
                new[] { InlineKeyboardButton.WithCallbackData($"Завантажити техпаспорт {vs}", "upload_vehicleRegistration_doc") }
            });

            session.CurrentState = DocumentState.AwaitingSelection;
            await _botClient.SendTextMessageAsync(
                session.ChatId,
                "Будь ласка, завантажте документи для оформлення страховки.",
                replyMarkup: kb,
                cancellationToken: cancellationToken);
        }
    }
}

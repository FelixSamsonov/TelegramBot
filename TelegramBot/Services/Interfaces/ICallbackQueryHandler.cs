using Telegram.Bot.Types;
using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions
{
    public interface ICallbackQueryHandler
    {
        Task HandleAsync(CallbackQuery query, UserSession session, CancellationToken cancellationToken);
    }
}

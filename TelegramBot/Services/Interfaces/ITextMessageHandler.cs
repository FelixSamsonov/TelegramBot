using Telegram.Bot.Types;
using TelegramBot.Models;

namespace TelegramBot.Services.Abs
{
    public interface ITextMessageHandler
    {
        Task HandleAsync(Message message, UserSession session, CancellationToken cancellationToken);
    }
}

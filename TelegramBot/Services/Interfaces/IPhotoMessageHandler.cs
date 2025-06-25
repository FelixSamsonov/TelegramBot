using Telegram.Bot.Types;
using TelegramBot.Models;

namespace TelegramBot.Services.Abs
{
    public interface IPhotoMessageHandler
    {
        Task HandleAsync(Message message, UserSession session, CancellationToken cancellationToken);
    }
}

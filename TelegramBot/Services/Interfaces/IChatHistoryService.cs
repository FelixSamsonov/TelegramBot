using TelegramBot.Models;

namespace TelegramBot.Services.Abs
{
    public interface IChatHistoryService
    {
        Task ClearHistoryAsync(UserSession session, CancellationToken cancellationToken);
    }
}

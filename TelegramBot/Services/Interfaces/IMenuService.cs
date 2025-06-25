using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions
{
    public interface IMenuService
    {
        Task SendMainMenuAsync(UserSession session, CancellationToken cancellationToken);
    }
}

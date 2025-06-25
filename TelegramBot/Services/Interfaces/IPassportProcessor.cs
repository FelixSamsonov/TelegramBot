using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions
{
    public interface IPassportProcessor
    {
        Task ProcessAsync(UserSession session, CancellationToken cancellationToken);
    }
}

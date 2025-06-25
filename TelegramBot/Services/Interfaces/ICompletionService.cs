using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions
{
    public interface ICompletionService
    {
        Task CheckCompletionAsync(UserSession session, CancellationToken cancellationToken);
        Task FinalizePolicyAsync(UserSession session, CancellationToken cancellationToken);
    }
}

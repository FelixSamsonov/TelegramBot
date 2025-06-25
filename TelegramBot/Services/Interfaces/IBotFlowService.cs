using Telegram.Bot.Types;

namespace TelegramBot.Services.Abstractions
{
    public interface IBotFlowService
    {
        Task ProcessUpdateAsync(Update update, CancellationToken cancellationToken);
    }
}

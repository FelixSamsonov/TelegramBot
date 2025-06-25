using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations.Handlers;

public class UpdateHandler : IUpdateHandler
{
    private readonly IBotFlowService _botFlowService;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(IBotFlowService botFlowService, ILogger<UpdateHandler> logger)
    {
        _botFlowService = botFlowService;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            await _botFlowService.ProcessUpdateAsync(update, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update {UpdateId}", update.Id);
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Polling error");
        return Task.CompletedTask;
    }
}

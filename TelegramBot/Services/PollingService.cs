using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
namespace TelegramBot.Services;

// Цей клас відповідає за запуск та зупинку отримання оновлень від Telegram
public class PollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PollingService> _logger;
    private readonly ITelegramBotClient _botClient;

    public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger, ITelegramBotClient botClient)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _botClient = botClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await _botClient.GetMeAsync(stoppingToken);
        _logger.LogInformation("Bot @{Username} started listening.", me.Username);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
            ThrowPendingUpdates = true,
        };

        await _botClient.ReceiveAsync(
            updateHandler: async (bot, update, ct) =>
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var updateHandler = scope.ServiceProvider.GetRequiredService<Abstractions.IUpdateHandler>();
                await updateHandler.HandleUpdateAsync(bot, update, ct);
            },
            pollingErrorHandler: async (bot, exception, ct) =>
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var updateHandler = scope.ServiceProvider.GetRequiredService<Abstractions.IUpdateHandler>();
                await updateHandler.HandlePollingErrorAsync(bot, exception, ct);
            },
            receiverOptions,
            cancellationToken: stoppingToken);
    }
}
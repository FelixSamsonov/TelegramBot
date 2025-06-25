using Telegram.Bot.Types;
using TelegramBot.Services.Abs;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations
{
    public class BotFlowService : IBotFlowService
    {
        private readonly IUserSessionService _sessionService;
        private readonly ITextMessageHandler _textHandler;
        private readonly IPhotoMessageHandler _photoHandler;
        private readonly ICallbackQueryHandler _callbackHandler;

        public BotFlowService(
            IUserSessionService sessionService,
            ITextMessageHandler textHandler,
            IPhotoMessageHandler photoHandler,
            ICallbackQueryHandler callbackHandler)
        {
            _sessionService = sessionService;
            _textHandler = textHandler;
            _photoHandler = photoHandler;
            _callbackHandler = callbackHandler;
        }

        public async Task ProcessUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            var chatId = update.Message?.Chat.Id
                      ?? update.CallbackQuery?.Message?.Chat.Id;
            if (chatId == null) return;

            var session = _sessionService.GetOrCreateSession(chatId.Value);
            session.ChatId = chatId.Value;

            if (update.Message?.MessageId != null)
                session.MessageIdsToDelete.Add(update.Message.MessageId);

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message != null)
            {
                if (update.Message.Text != null)
                    await _textHandler.HandleAsync(update.Message, session, cancellationToken);
                else if (update.Message.Photo != null)
                    await _photoHandler.HandleAsync(update.Message, session, cancellationToken);
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await _callbackHandler.HandleAsync(update.CallbackQuery, session, cancellationToken);
            }
        }
    }
}

using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Models;
using TelegramBot.Services.Abs;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations.Handlers
{
    public class TextMessageHandler : ITextMessageHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IOpenAiService _openAiService;
        private readonly IMenuService _menuService;
        private readonly IChatHistoryService _chatHistoryService;
        private readonly IUserSessionService _userSessionService;

        public TextMessageHandler(
            ITelegramBotClient botClient,
            IOpenAiService openAiService,
            IMenuService menuService,
            IChatHistoryService chatHistoryService,
            IUserSessionService userSessionService )
        {
            _botClient = botClient;
            _openAiService = openAiService;
            _menuService = menuService;
            _chatHistoryService = chatHistoryService;
            _userSessionService = userSessionService;
        }

        public async Task HandleAsync(Message message, UserSession session, CancellationToken cancellationToken)
        {
            var text = message?.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            if (text.Equals("/stop_chat", StringComparison.OrdinalIgnoreCase)
                && session.CurrentState == DocumentState.AwaitingQuestion)
            {
                session.CurrentState = DocumentState.AwaitingSelection;
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "✅ Режим консультації завершено. Повертаємось до головного меню.",
                    cancellationToken: cancellationToken);
                await _menuService.SendMainMenuAsync(session, cancellationToken);
                return;
            }

            if (session.CurrentState == DocumentState.AwaitingQuestion)
            {
                await _botClient.SendChatActionAsync(session.ChatId, ChatAction.Typing, cancellationToken);
                var aiResponse = await _openAiService.GetGenericResponseAsync(text, cancellationToken)
                                       ?? "Вибачте, сталася помилка. Спробуйте поставити питання трохи інакше.";
                aiResponse += "\n\n🛑 Щоб завершити консультацію, надішліть /stop_chat";
                await _botClient.SendTextMessageAsync(session.ChatId, aiResponse, cancellationToken: cancellationToken);
                return;
            }

            switch (text.ToLower())
            {
                case "/start":
                    _userSessionService.ResetSession(session.ChatId);
                    session = _userSessionService.GetOrCreateSession(session.ChatId);

                    await _chatHistoryService.ClearHistoryAsync(session, cancellationToken);
                    var startMessage = "Привіт! Я ваш помічник для оформлення автосрахування." +
                        " 🚗\r\n\r\nМоя мета — допомогти вам швидко та зручно оформити страховий поліс для вашого транспортного засобу." +
                        " Просто надішліть необхідні документи, і я все зроблю за вас!";
                    await _botClient.SendTextMessageAsync(session.ChatId,
                        startMessage,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    await _menuService.SendMainMenuAsync(session, cancellationToken);
                    break;
                case "/info":
                    var info = new StringBuilder()
                        .AppendLine("🤖 **Що я вмію:**")
                        .AppendLine()
                        .AppendLine("1️⃣ Оформлення полісу:** Я допоможу вам швидко оформити страховий поліс, вам тільки треба надіслати фото документів./start")
                        .AppendLine("2️⃣ Консультація:** Я можу відповісти на ваші питання щодо страхування в Україні./conversation")
                        .AppendLine("3️⃣ Очищення:** Якщо ви хочете видалити недавню історію чату, надішліть команду /clear.")
                        .ToString();
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        info,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    break;
                case "/conversation":
                    session.CurrentState = DocumentState.AwaitingQuestion;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "Ви перейшли в режим консультації з ШІ-експертом. 💬\n\n" +
                        "Просто напишіть ваше питання в чат.\n\n" +
                        "Щоб відразу завершити консультацію – надішліть /stop_chat.",
                        cancellationToken: cancellationToken);
                    break;
                case "/clear":
                    await _chatHistoryService.ClearHistoryAsync(session, cancellationToken);
                    break;
            }
        }
    }
}

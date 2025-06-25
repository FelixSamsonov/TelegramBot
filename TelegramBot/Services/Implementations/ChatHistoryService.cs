using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.Services.Abs;

namespace TelegramBot.Services.Imp
{
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly ITelegramBotClient _botClient;

        public ChatHistoryService(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task ClearHistoryAsync(UserSession session, CancellationToken cancellationToken)
        {
            var toDelete = new List<int>(session.MessageIdsToDelete);
            session.MessageIdsToDelete.Clear();

            var wait = await _botClient.SendTextMessageAsync(
                session.ChatId,
                $"⏳ Видаляю {toDelete.Count} повідомлень…",
                cancellationToken: cancellationToken);
            toDelete.Add(wait.MessageId);

            int deleted = 0;
            foreach (var msgId in toDelete)
            {
                try
                {
                    await _botClient.DeleteMessageAsync(session.ChatId, msgId, cancellationToken);
                    deleted++;
                }
                catch { }
            }

            var fin = await _botClient.SendTextMessageAsync(
                session.ChatId,
                $"✅ Видалено {deleted} повідомлень.\nНадішліть /start для нової сесії.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
            session.MessageIdsToDelete.Add(fin.MessageId);
        }
    }
}

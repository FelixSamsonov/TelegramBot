using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions;

public interface IUserSessionService
{
    /// <summary>
    /// Отримує існуючу сесію для чату або створює нову.
    /// </summary>
    /// <param name="chatId">ID чату.</param>
    /// <returns>Сесія користувача.</returns>
    UserSession GetOrCreateSession(long chatId);

    /// <summary>
    /// Скидає сесію для чату до початкового стану.
    /// </summary>
    /// <param name="chatId">ID чату.</param>
    void ResetSession(long chatId);
}
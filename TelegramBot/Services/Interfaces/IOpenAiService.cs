using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions;

public interface IOpenAiService
{
    /// <summary>
    /// Генерує текст страхового полісу на основі даних користувача та автомобіля.
    /// </summary>
    /// <param name="session">Сесія користувача з фінальними даними.</param>
    /// <param name="cancellationToken">Токен скасування.</param>
    /// <returns>Рядок з текстом згенерованого полісу або null у разі помилки.</returns>
    Task<string?> GenerateInsurancePolicyTextAsync(UserSession session, CancellationToken cancellationToken);
    /// <summary>
    /// Отримує відповідь від OpenAI на довільне питання користувача про страхування.
    /// </summary>
    /// <param name="question">Питання від користувача.</param>
    /// <param name="cancellationToken">Токен скасування.</param>
    /// <returns>Відповідь від моделі.</returns>
    Task<string?> GetGenericResponseAsync(string question, CancellationToken cancellationToken);
}

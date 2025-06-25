using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions;

public interface IFileService
{
    /// <summary>
    /// Завантажує фото з Telegram за його FileId.
    /// </summary>
    /// <param name="fileId">ID файлу в Telegram.</param>
    /// <param name="cancellationToken">Токен скасування.</param>
    /// <returns>Шлях до збереженого тимчасового файлу або null у разі помилки.</returns>
    Task<string?> DownloadPhotoAsync(string fileId, CancellationToken cancellationToken);

    /// <summary>
    /// Видаляє всі тимчасові файли, пов'язані з сесією.
    /// </summary>
    /// <param name="session">Сесія, файли якої потрібно видалити.</param>
    void DeleteSessionFiles(UserSession session);

    /// <summary>
    /// Створює PDF-файл з двох зображень.
    /// </summary>
    /// <param name="imagePath1">Шлях до першого зображення.</param>
    /// <param name="imagePath2">Шлях до другого зображення.</param>
    /// <returns>Шлях до створеного PDF-файлу.</returns>
    string CreatePdfFromImages(string imagePath1, string imagePath2);
}
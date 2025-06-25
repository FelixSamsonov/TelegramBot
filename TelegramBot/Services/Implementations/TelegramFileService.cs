using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Telegram.Bot;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations
{
    public class TelegramFileService : IFileService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramFileService> _logger;

        public TelegramFileService(
            ITelegramBotClient botClient,
            ILogger<TelegramFileService> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        // Завантажує фото з Telegram за його FileId.
        public async Task<string?> DownloadPhotoAsync(string fileId, CancellationToken cancellationToken)
        {
            try
            {
                var file = await _botClient.GetFileAsync(fileId, cancellationToken);

                if (string.IsNullOrWhiteSpace(file.FilePath))
                {
                    _logger.LogError("TelegramFileService: FilePath is empty for fileId {FileId}", fileId);
                    return null;
                }

                var ext = Path.GetExtension(file.FilePath); 
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{ext}");

                await using var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await _botClient.DownloadFileAsync(file.FilePath, fs, cancellationToken);

                return tempFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TelegramFileService: Error downloading file {FileId}", fileId);
                return null;
            }
        }

        [Obsolete]
        public string CreatePdfFromImages(string firstImagePath, string secondImagePath)
        {
            var pdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

            using var document = new PdfDocument();

            foreach (var imgPath in new[] { firstImagePath, secondImagePath })
            {
                if (!File.Exists(imgPath))
                    throw new FileNotFoundException("Image file not found", imgPath);

                var page = document.AddPage();

                using var xImage = XImage.FromFile(imgPath);

                page.Width = xImage.PointWidth;
                page.Height = xImage.PointHeight;

                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawImage(xImage, 0, 0, page.Width, page.Height);
            }

            document.Save(pdfPath);
            return pdfPath;
        }

        public void DeleteSessionFiles(UserSession session)
        {
            void TryDelete(string? path)
            {
                if (string.IsNullOrWhiteSpace(path)) return;
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "TelegramFileService: Failed to delete temp file {Path}", path);
                }
            }

            TryDelete(session.PassportFrontImagePath);
            TryDelete(session.PassportBackImagePath);
            TryDelete(session.VehicleRegistrationFrontImagePath);
            TryDelete(session.VehicleRegistrationBackImagePath);
        }
    }
}

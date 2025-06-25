using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Business
{
    // Клас, який обробляє документи паспорта користувача.
    // Використовує Mindee для розпізнавання даних з фото паспорт
    public class PassportProcessor : IPassportProcessor
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IFileService _fileService;
        private readonly IMindeeService _mindeeService;

        public PassportProcessor(
            ITelegramBotClient botClient,
            IFileService fileService,
            IMindeeService mindeeService)
        {
            _botClient = botClient;
            _fileService = fileService;
            _mindeeService = mindeeService;
        }

        // Обробляє фото паспорта, створює PDF та розпізнає дані.
        public async Task ProcessAsync(UserSession session, CancellationToken cancellationToken)
        {
            if (session.PassportFrontImagePath == null ||
                session.PassportBackImagePath == null)
            {
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "❌ Не вистачає обох фото паспорта.",
                    cancellationToken: cancellationToken);
                session.CurrentState = DocumentState.AwaitingPassportFront;
                return;
            }
            // Створення PDF з двох зображень. Створює тимчасовий файл PDF з двох зображень паспорта і відправляє його на Mindee для розпізнавання даних.
            string? pdf = null;
            try
            {
                pdf = _fileService.CreatePdfFromImages(
                    session.PassportFrontImagePath!,
                    session.PassportBackImagePath!);

                var parsedData = await _mindeeService.ProcessPassportAsync(pdf, cancellationToken);

                if (parsedData == null)
                {
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "❌ Не вдалося розпізнати паспорт. Спробуйте інші фото.",
                        cancellationToken: cancellationToken);
                    session.CurrentState = DocumentState.AwaitingPassportFront;
                    return;
                }

                session.PendingPassportData = parsedData;

                var passportInfo = new StringBuilder();
                passportInfo.AppendLine("📋 Перевірте, будь ласка, розпізнану інформацію з паспорта:");
                if (!string.IsNullOrEmpty(parsedData.Surname)) passportInfo.AppendLine($"👤 Прізвище: {parsedData.Surname}");
                if (!string.IsNullOrEmpty(parsedData.Name)) passportInfo.AppendLine($"👤 Ім'я: {parsedData.Name}");
                if (!string.IsNullOrEmpty(parsedData.GivenName)) passportInfo.AppendLine($"👤 По-батькові: {parsedData.GivenName}");
                if (!string.IsNullOrEmpty(parsedData.DateOfBirth)) passportInfo.AppendLine($"📅 Дата народження: {parsedData.DateOfBirth}");
                if (!string.IsNullOrEmpty(parsedData.DocumentNumber)) passportInfo.AppendLine($"📜 Номер документа: {parsedData.DocumentNumber}");
                if (!string.IsNullOrEmpty(parsedData.RecordNumber)) passportInfo.AppendLine($"📜 Номер запису: {parsedData.RecordNumber}");
                if (!string.IsNullOrEmpty(parsedData.RegisteredAddress))
                    passportInfo.AppendLine($"📜 Адреса реєстрації: {parsedData.RegisteredAddress}");
                if (!string.IsNullOrEmpty(parsedData.Rntrc)) passportInfo.AppendLine($"РНОКПП: {parsedData.Rntrc}");

                passportInfo.AppendLine("\nЧи вірні дані?");

                var kb = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("✅ Так", "confirm_passport_data") },
                    new[] { InlineKeyboardButton.WithCallbackData("❌ Ні", "retry_passport_data") }
                });

                session.CurrentState = DocumentState.AwaitingPassportConfirmation;
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    passportInfo.ToString(),
                    replyMarkup: kb,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                // Логування помилки
                Console.WriteLine($"Error: {ex.Message}");
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "❌ Помилка сервера. Спробуйте пізніше.",
                    cancellationToken: cancellationToken);
            }
            finally
            {
                // Очищення тимчасових файлів
                if (pdf != null && File.Exists(pdf))
                    File.Delete(pdf);
            }
        }
    }
}

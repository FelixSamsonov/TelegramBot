using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Business
{
    public class VehicleRegistrationProcessor : IVehicleRegistrationProcessor
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IFileService _fileService;
        private readonly IMindeeService _mindeeService;

        public VehicleRegistrationProcessor(
            ITelegramBotClient botClient,
            IFileService fileService,
            IMindeeService mindeeService)
        {
            _botClient = botClient;
            _fileService = fileService;
            _mindeeService = mindeeService;
        }

        public async Task ProcessAsync(UserSession session, CancellationToken cancellationToken)
        {
            if (session.VehicleRegistrationFrontImagePath == null ||
                session.VehicleRegistrationBackImagePath == null)
            {
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "❌ Не вистачає обох фото техпаспорта.",
                    cancellationToken: cancellationToken);
                session.CurrentState = DocumentState.AwaitingVehicleRegistrationFront;
                return;
            }

            string? pdf = null;
            try
            {
                pdf = _fileService.CreatePdfFromImages(
                    session.VehicleRegistrationFrontImagePath!,
                    session.VehicleRegistrationBackImagePath!);

                var parsedData = await _mindeeService.ProcessCarRegistrationAsync(pdf, cancellationToken);
                if (parsedData == null)
                {
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "❌ Не вдалося розпізнати техпаспорт.",
                        cancellationToken: cancellationToken);
                    session.CurrentState = DocumentState.AwaitingVehicleRegistrationFront;
                    return;
                }

                session.PendingVehicleRegistrationData = parsedData;

                var carInfo = new StringBuilder();
                carInfo.AppendLine("📋 Перевірте, будь ласка, інформацію з техпаспорта:");
                if (!string.IsNullOrEmpty(parsedData.VehicleMake)) carInfo.AppendLine($"🚗 Марка: {parsedData.VehicleMake}");
                if (!string.IsNullOrEmpty(parsedData.VehicleModel)) carInfo.AppendLine($"🏷️ Модель: {parsedData.VehicleModel}");
                if (!string.IsNullOrEmpty(parsedData.LicensePlate)) carInfo.AppendLine($"🔢 Номерний знак: {parsedData.LicensePlate}");
                if (!string.IsNullOrEmpty(parsedData.VinCode)) carInfo.AppendLine($"🔍 VIN: {parsedData.VinCode}");
                if (!string.IsNullOrEmpty(parsedData.OwnerSurname)) carInfo.AppendLine($"👤 Прізвище власника: {parsedData.OwnerSurname}");
                if (!string.IsNullOrEmpty(parsedData.OwnerGivenNames)) carInfo.AppendLine($"👤 Ім'я власника: {parsedData.OwnerGivenNames}");
                if (!string.IsNullOrEmpty(parsedData.OwnerAddress)) carInfo.AppendLine($"🏠 Адреса власника: {parsedData.OwnerAddress}");
                if (!string.IsNullOrEmpty(parsedData.YearOfManufacture)) carInfo.AppendLine($"📅 Рік випуску: {parsedData.YearOfManufacture}");
                if (!string.IsNullOrEmpty(parsedData.DocumentId)) carInfo.AppendLine($"📜 ID документа: {parsedData.DocumentId}");
                if (!string.IsNullOrEmpty(parsedData.FirstRegistrationDate)) carInfo.AppendLine($"📅 Дата першої реєстрації: {parsedData.FirstRegistrationDate}");
                carInfo.AppendLine("\nЧи вірні дані?");

                var kb = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("✅ Так",   "confirm_vehicleRegistration_data") },
                    new[] { InlineKeyboardButton.WithCallbackData("❌ Ні",   "retry_vehicleRegistration_data") }
                });

                session.CurrentState = DocumentState.AwaitingVehicleRegistrationConfirmation;
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    carInfo.ToString(),
                    replyMarkup: kb,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "❌ Помилка сервера. Спробуйте пізніше.",
                    cancellationToken: cancellationToken);
            }
            finally
            {
                if (pdf != null && File.Exists(pdf))
                    File.Delete(pdf);
            }
        }
    }
}

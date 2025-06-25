using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Models;
using TelegramBot.Services.Abs;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations.Handlers
{
    public class PhotoMessageHandler : IPhotoMessageHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IFileService _fileService;
        private readonly IPassportProcessor _passportProcessor;
        private readonly IVehicleRegistrationProcessor _vehicleProcessor;

        public PhotoMessageHandler(
            ITelegramBotClient botClient,
            IFileService fileService,
            IPassportProcessor passportProcessor,
            IVehicleRegistrationProcessor vehicleProcessor)
        {
            _botClient = botClient;
            _fileService = fileService;
            _passportProcessor = passportProcessor;
            _vehicleProcessor = vehicleProcessor;
        }

        public async Task HandleAsync(Message message, UserSession session, CancellationToken cancellationToken)
        {
            var fileId = message.Photo?.LastOrDefault()?.FileId;
            if (fileId is null)
            {
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "❌ Не вдалося отримати фото. Спробуйте ще раз.",
                    cancellationToken: cancellationToken);
                return;
            }

            var expecting = session.CurrentState is
                DocumentState.AwaitingPassportFront or
                DocumentState.AwaitingPassportBack or
                DocumentState.AwaitingVehicleRegistrationFront or
                DocumentState.AwaitingVehicleRegistrationBack;

            if (!expecting)
            {
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "Будь ласка, оберіть документ через /start.",
                    cancellationToken: cancellationToken);
                return;
            }

            await _botClient.SendTextMessageAsync(session.ChatId, "Обробляю фото…", cancellationToken: cancellationToken);
            var path = await _fileService.DownloadPhotoAsync(fileId, cancellationToken);
            if (path == null)
            {
                await _botClient.SendTextMessageAsync(
                    session.ChatId,
                    "❌ Не вдалося завантажити файл.",
                    cancellationToken: cancellationToken);
                return;
            }

            switch (session.CurrentState)
            {
                case DocumentState.AwaitingPassportFront:
                    session.PassportFrontImagePath = path;
                    session.CurrentState = DocumentState.AwaitingPassportBack;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "✅ Перша сторона паспорта отримана.\nНадішліть другу сторону.",
                        cancellationToken: cancellationToken);
                    break;

                case DocumentState.AwaitingPassportBack:
                    session.PassportBackImagePath = path;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "✅ Друга сторона паспорта отримана. Обробляю…",
                        cancellationToken: cancellationToken);
                    await _passportProcessor.ProcessAsync(session, cancellationToken);
                    break;

                case DocumentState.AwaitingVehicleRegistrationFront:
                    session.VehicleRegistrationFrontImagePath = path;
                    session.CurrentState = DocumentState.AwaitingVehicleRegistrationBack;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "✅ Перша сторона техпаспорта отримана.\nНадішліть другу сторону.",
                        cancellationToken: cancellationToken);
                    break;

                case DocumentState.AwaitingVehicleRegistrationBack:
                    session.VehicleRegistrationBackImagePath = path;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "✅ Друга сторона техпаспорта отримана. Обробляю…",
                        cancellationToken: cancellationToken);
                    await _vehicleProcessor.ProcessAsync(session, cancellationToken);
                    break;
            }
        }
    }
}

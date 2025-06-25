using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations.Handlers
{
    public class CallbackQueryHandler : ICallbackQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ICompletionService _completionService;

        public CallbackQueryHandler(
            ITelegramBotClient botClient,
            ICompletionService completionService)
        {
            _botClient = botClient;
            _completionService = completionService;
        }

        public async Task HandleAsync(CallbackQuery query, UserSession session, CancellationToken cancellationToken)
        {
            if (query.Data == null || query.Message == null) return;

            if (session.CurrentState == DocumentState.AwaitingQuestion)
            {
                await _botClient.AnswerCallbackQueryAsync(
                    query.Id,
                    "Завершіть консультацію: /stop_chat",
                    showAlert: true,
                    cancellationToken: cancellationToken);
                return;
            }

            await _botClient.AnswerCallbackQueryAsync(query.Id, cancellationToken: cancellationToken);
            try
            {
                await _botClient.EditMessageReplyMarkupAsync(
                    session.ChatId,
                    query.Message.MessageId,
                    replyMarkup: null,
                    cancellationToken: cancellationToken);
            }
            catch { }

            switch (query.Data)
            {
                case "upload_passport":
                    session.CurrentState = DocumentState.AwaitingPassportFront;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "📋 Надішліть фото першої сторони, або відразу дві сторони паспорта",
                        cancellationToken: cancellationToken);
                    break;

                case "upload_vehicleRegistration_doc":
                    session.CurrentState = DocumentState.AwaitingVehicleRegistrationFront;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "📋 Надішліть фото першої сторони, або відразу дві сторони техпаспорта.",
                        cancellationToken: cancellationToken);
                    break;

                case "confirm_passport_data":
                    session.PassportData = session.PendingPassportData;
                    session.PendingPassportData = null;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "✅ Дані паспорта підтверджено.",
                        cancellationToken: cancellationToken);
                    await _completionService.CheckCompletionAsync(session, cancellationToken);
                    break;

                case "retry_passport_data":
                    session.CurrentState = DocumentState.AwaitingPassportFront;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "❌ Спробуйте ще раз. Надішліть фото першої сторони.",
                        cancellationToken: cancellationToken);
                    break;

                case "confirm_vehicleRegistration_data":
                    session.VehicleRegistrationData = session.PendingVehicleRegistrationData;
                    session.PendingVehicleRegistrationData = null;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "✅ Дані техпаспорта підтверджено.",
                        cancellationToken: cancellationToken);
                    await _completionService.CheckCompletionAsync(session, cancellationToken);
                    break;

                case "retry_vehicleRegistration_data":
                    session.CurrentState = DocumentState.AwaitingVehicleRegistrationFront;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "❌ Спробуйте ще раз. Надішліть фото першої сторони.",
                        cancellationToken: cancellationToken);
                    break;

                case "confirm_price":
                    await _completionService.FinalizePolicyAsync(session, cancellationToken);
                    break;

                case "decline_price":
                    session.CurrentState = DocumentState.AwaitingSelection;
                    await _botClient.SendTextMessageAsync(
                        session.ChatId,
                        "Нажаль, інша ціна недоступна. Спробуйте знову /start.",
                        cancellationToken: cancellationToken);
                    break;
            }
        }
    }
}

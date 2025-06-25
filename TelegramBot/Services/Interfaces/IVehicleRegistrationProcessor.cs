using TelegramBot.Models;

namespace TelegramBot.Services.Abstractions
{
    public interface IVehicleRegistrationProcessor
    {
        Task ProcessAsync(UserSession session, CancellationToken cancellationToken);
    }
}

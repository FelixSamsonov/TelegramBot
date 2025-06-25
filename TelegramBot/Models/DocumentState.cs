namespace TelegramBot.Models
{
    public enum DocumentState
    {
        AwaitingSelection,
        AwaitingPassportFront,
        AwaitingPassportBack,
        AwaitingVehicleRegistrationFront,
        AwaitingVehicleRegistrationBack,
        AwaitingPassportConfirmation,
        AwaitingVehicleRegistrationConfirmation,
        AwaitingPriceConfirmation,
        AwaitingQuestion
    }
}

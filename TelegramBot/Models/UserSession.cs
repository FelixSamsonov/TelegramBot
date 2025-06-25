namespace TelegramBot.Models
{
    public class UserSession
    {
        public long ChatId { get; set; }

        public DocumentState CurrentState { get; set; } = DocumentState.AwaitingSelection;

        // Дані паспорта
        public string? PassportFrontImagePath { get; set; }
        public string? PassportBackImagePath { get; set; }
        public PersonData? PassportData { get; set; }
        public PersonData? PendingPassportData { get; set; }

        // Дані техпаспорта
        public string? VehicleRegistrationFrontImagePath { get; set; }
        public string? VehicleRegistrationBackImagePath { get; set; }
        public VehicleRegistration? VehicleRegistrationData { get; set; }
        public VehicleRegistration? PendingVehicleRegistrationData { get; set; }

        public bool IsComplete() => PassportData != null && VehicleRegistrationData != null;
        public List<int> MessageIdsToDelete { get; set; } = new();

    }
}

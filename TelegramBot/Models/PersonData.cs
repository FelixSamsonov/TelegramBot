namespace TelegramBot.Models
{
    public class PersonData
    {
        public string? Surname { get; set; }
        public string? Name { get; set; }
        public string? GivenName { get; set; }
        public string? DateOfBirth { get; set; }
        public string? DocumentNumber { get; set; }
        public string? RecordNumber { get; set; }
        public string? RegisteredAddress { get; set; }
        public string? Rntrc { get; set; }

        public string FullName => $"{Surname} {Name} {GivenName}".Trim();
    }
}

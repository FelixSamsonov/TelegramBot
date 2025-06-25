using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Text;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations;

public class OpenAiService : IOpenAiService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(ChatClient chatClient, ILogger<OpenAiService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

      public async Task<string?> GetGenericResponseAsync(string question, CancellationToken cancellationToken)
    {

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("Ти помічник і експерт зі страхування в Україні. Відповідай на питання користувачів чітко та інформативно українською мовою."),
                new UserChatMessage(question)
            };
            var options = new ChatCompletionOptions { Temperature = 0.7f };
            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling OpenAI API for a generic question.");
            return null;
        }
    }

    public async Task<string?> GenerateInsurancePolicyTextAsync(UserSession session, CancellationToken cancellationToken)
    {
        if (session.PassportData is null || session.VehicleRegistrationData is null)
        {
            _logger.LogWarning("Attempted to generate policy with incomplete data for session.");
            return null;
        }
        string prompt = CreatePromptFromTemplate(session);
        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("Ти є асистентом для генерації страхових полісів. Дотримуйся точно заданого формату та не додавай нічого від себе."),
                new UserChatMessage(prompt)
            };
            var options = new ChatCompletionOptions { Temperature = 0.2f };
            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calling OpenAI API.");
            return null;
        }
    }

    private string CreatePromptFromTemplate(UserSession session)
    {
        var passport = session.PassportData!;
        var vehicle = session.VehicleRegistrationData!;
        var sb = new StringBuilder();
        sb.AppendLine("Згенеруй текст страхового полісу ОСЦПВ на основі наступних даних. Дотримуйся ТОЧНО цього формату:");
        sb.AppendLine();
        sb.AppendLine("=== СТРАХОВИЙ ПОЛІС № TG-2024-1001 ===");
        sb.AppendLine("Обов'язкове страхування цивільно-правової відповідальності власників наземних транспортних засобів (ОСЦПВ)");
        sb.AppendLine();
        sb.AppendLine("1. СТРАХУВАЛЬНИК:");
        sb.AppendLine($"   ПІБ: {passport.FullName}");
        sb.AppendLine($"   Дата народження: {passport.DateOfBirth ?? "не вказано"}");
        sb.AppendLine($"   Адреса реєстрації: {passport.RegisteredAddress ?? "не вказано"}");
        sb.AppendLine($"   РНОКПП: {passport.Rntrc ?? "не вказано"}");
        sb.AppendLine();
        sb.AppendLine("2. ТРАНСПОРТНИЙ ЗАСІБ:");
        sb.AppendLine($"   Марка/Модель: {vehicle.VehicleMake ?? ""} {vehicle.VehicleModel ?? ""}");
        sb.AppendLine($"   Державний номер: {vehicle.LicensePlate ?? "не вказано"}");
        sb.AppendLine($"   VIN-код: {vehicle.VinCode ?? "не вказано"}");
        sb.AppendLine($"   Рік випуску: {vehicle.YearOfManufacture ?? "не вказано"}");
        sb.AppendLine();
        sb.AppendLine("3. УМОВИ СТРАХУВАННЯ:");
        sb.AppendLine("   Страхова сума (майно): 160 000,00 грн");
        sb.AppendLine("   Страхова сума (життя/здоров'я): 320 000,00 грн");
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddYears(1).AddDays(-1);
        sb.AppendLine($"   Строк дії: з 00:00 {startDate:dd.MM.yyyy} до 23:59 {endDate:dd.MM.yyyy}");
        sb.AppendLine("   Вартість полісу: 100 USD");
        sb.AppendLine();
        sb.AppendLine("4. ПІДТВЕРДЖЕННЯ:");
        sb.AppendLine("Поліс набуває чинності після повної оплати страхового платежу.");
        sb.AppendLine($"Дата генерації: {DateTime.Now:dd.MM.yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("Поверни тільки заповнений поліс без додаткових коментарів.");
        return sb.ToString();
    }
}
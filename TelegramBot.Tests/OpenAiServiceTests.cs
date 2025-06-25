using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;
using TelegramBot.Services.Implementations;
using Xunit;

namespace TelegramBot.Tests;

public class OpenAiServiceTests
{
    private readonly Mock<ILogger<OpenAiService>> _mockLogger;

    public OpenAiServiceTests()
    {
        _mockLogger = new Mock<ILogger<OpenAiService>>();
    }

    [Fact]
    public async Task GenerateInsurancePolicyTextAsync_ShouldReturnNull_WhenPassportDataIsNull()
    {
        // Arrange
        var mockChatClient = new Mock<OpenAI.Chat.ChatClient>();
        var service = new OpenAiService(mockChatClient.Object, _mockLogger.Object);

        var session = new UserSession
        {
            PassportData = null,
            VehicleRegistrationData = new VehicleRegistration { VehicleMake = "TestVehicle" }
        };

        // Act
        var result = await service.GenerateInsurancePolicyTextAsync(session, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        // Перевіряємо, що було залоговано попередження
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Перевіряємо, що ChatClient не викликався
        mockChatClient.Verify(x => x.CompleteChatAsync(
            It.IsAny<System.Collections.Generic.IEnumerable<OpenAI.Chat.ChatMessage>>(),
            It.IsAny<OpenAI.Chat.ChatCompletionOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateInsurancePolicyTextAsync_ShouldReturnNull_WhenVehicleDataIsNull()
    {
        // Arrange
        var mockChatClient = new Mock<OpenAI.Chat.ChatClient>();
        var service = new OpenAiService(mockChatClient.Object, _mockLogger.Object);

        var session = new UserSession
        {
            PassportData = new PersonData { Surname = "TestSurname", Name = "TestName" },
            VehicleRegistrationData = null
        };

        // Act
        var result = await service.GenerateInsurancePolicyTextAsync(session, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        // Перевіряємо, що було залоговано попередження
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Перевіряємо, що ChatClient не викликався
        mockChatClient.Verify(x => x.CompleteChatAsync(
            It.IsAny<IEnumerable<OpenAI.Chat.ChatMessage>>(),
            It.IsAny<OpenAI.Chat.ChatCompletionOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateInsurancePolicyTextAsync_ShouldReturnNull_WhenBothDataAreNull()
    {
        // Arrange
        var mockChatClient = new Mock<OpenAI.Chat.ChatClient>();
        var service = new OpenAiService(mockChatClient.Object, _mockLogger.Object);

        var session = new UserSession();

        // Act
        var result = await service.GenerateInsurancePolicyTextAsync(session, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        // Перевіряємо, що було залоговано попередження
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateInsurancePolicyTextAsync_ShouldReturnNull_WhenExceptionOccurs()
    {
        // Arrange
        var mockChatClient = new Mock<OpenAI.Chat.ChatClient>();
        mockChatClient
            .Setup(x => x.CompleteChatAsync(
                It.IsAny<IEnumerable<OpenAI.Chat.ChatMessage>>(),
                It.IsAny<OpenAI.Chat.ChatCompletionOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        var service = new OpenAiService(mockChatClient.Object, _mockLogger.Object);

        var session = new UserSession
        {
            PassportData = new PersonData { Surname = "TestSurname", Name = "TestName" },
            VehicleRegistrationData = new VehicleRegistration { VehicleMake = "TestVehicle" }
        };

        // Act
        var result = await service.GenerateInsurancePolicyTextAsync(session, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        // Перевіряємо, що помилка була залогована
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetGenericResponseAsync_ShouldReturnNull_WhenExceptionOccurs()
    {
        // Arrange
        var question = "Тестове питання";
        var mockChatClient = new Mock<OpenAI.Chat.ChatClient>();
        mockChatClient
            .Setup(x => x.CompleteChatAsync(
                It.IsAny<System.Collections.Generic.IEnumerable<OpenAI.Chat.ChatMessage>>(),
                It.IsAny<OpenAI.Chat.ChatCompletionOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        var service = new OpenAiService(mockChatClient.Object, _mockLogger.Object);

        // Act
        var result = await service.GetGenericResponseAsync(question, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        // Перевіряємо, що помилка була залогована
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GenerateInsurancePolicyTextAsync_ShouldValidateInputData()
    {
        // Arrange
        var mockChatClient = new Mock<OpenAI.Chat.ChatClient>();
        var service = new OpenAiService(mockChatClient.Object, _mockLogger.Object);

        // Act & Assert - тестуємо різні комбінації null значень
        var sessionWithNullPassport = new UserSession
        {
            PassportData = null,
            VehicleRegistrationData = new VehicleRegistration()
        };

        var sessionWithNullVehicle = new UserSession
        {
            PassportData = new PersonData(),
            VehicleRegistrationData = null
        };

        var emptySession = new UserSession();

        // Всі ці сесії повинні повертати null без виклику API
        var tasks = new[]
        {
            service.GenerateInsurancePolicyTextAsync(sessionWithNullPassport, CancellationToken.None),
            service.GenerateInsurancePolicyTextAsync(sessionWithNullVehicle, CancellationToken.None),
            service.GenerateInsurancePolicyTextAsync(emptySession, CancellationToken.None)
        };

        Task.WaitAll(tasks);

        foreach (var task in tasks)
        {
            task.Result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetGenericResponseAsync_ShouldAcceptValidInput()
    {
        // Arrange
        var mockChatClient = new Mock<OpenAI.Chat.ChatClient>();
        var service = new OpenAiService(mockChatClient.Object, _mockLogger.Object);

        // Налаштовуємо мок для викидання винятку, щоб тест не залежав від реального API
        mockChatClient
            .Setup(x => x.CompleteChatAsync(
                It.IsAny<IEnumerable<OpenAI.Chat.ChatMessage>>(),
                It.IsAny<OpenAI.Chat.ChatCompletionOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Expected test exception"));

        // Act
        var result = await service.GetGenericResponseAsync("Test question", CancellationToken.None);

        // Assert
        result.Should().BeNull();

        // Перевіряємо, що метод спробував викликати API
        mockChatClient.Verify(x => x.CompleteChatAsync(
            It.Is<IEnumerable<OpenAI.Chat.ChatMessage>>(messages =>
                messages.Count() == 2), 
            It.IsAny<OpenAI.Chat.ChatCompletionOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
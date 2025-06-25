using FluentAssertions;
using TelegramBot.Models;
using TelegramBot.Services.Implementations;


namespace TelegramBot.Tests;

public class InMemoryUserSessionServiceTests
{
    private InMemoryUserSessionService CreateService()
    {
        return new InMemoryUserSessionService();
    }

    [Fact] 
    public void GetOrCreateSession_ShouldCreateNewSession_ForNewChatId()
    {
        // Arrange 
        var service = CreateService();
        long newChatId = 12345;

        // Act 
        var session = service.GetOrCreateSession(newChatId);

        // Assert 
        session.Should().NotBeNull(); 
        session.ChatId.Should().Be(newChatId); 
        session.CurrentState.Should().Be(DocumentState.AwaitingSelection); 
    }

    [Fact]
    public void GetOrCreateSession_ShouldReturnExistingSession_ForExistingChatId()
    {
        // Arrange
        var service = CreateService();
        long chatId = 12345;

        // Створюємо першу сесію
        var firstSession = service.GetOrCreateSession(chatId);
        firstSession.CurrentState = DocumentState.AwaitingPassportFront; // Змінюємо стан для перевірки

        // Act
        // Викликаємо метод вдруге для того ж самого chatId
        var secondSession = service.GetOrCreateSession(chatId);

        // Assert
        secondSession.Should().NotBeNull();
        secondSession.Should().BeSameAs(firstSession); 
        secondSession.CurrentState.Should().Be(DocumentState.AwaitingPassportFront);
    }

    [Fact]
    public void ResetSession_ShouldCreateNewSessionWithDefaultState()
    {
        // Arrange
        var service = CreateService();
        long chatId = 12345;

        var originalSession = service.GetOrCreateSession(chatId);
        originalSession.CurrentState = DocumentState.AwaitingPriceConfirmation; 
        originalSession.PassportData = new PersonData { Name = "Test" }; 

        // Act
        service.ResetSession(chatId);
        var newSession = service.GetOrCreateSession(chatId);

        // Assert
        newSession.Should().NotBeNull();
        newSession.Should().NotBeSameAs(originalSession); 
        newSession.CurrentState.Should().Be(DocumentState.AwaitingSelection); 
        newSession.PassportData.Should().BeNull(); 
    }
}
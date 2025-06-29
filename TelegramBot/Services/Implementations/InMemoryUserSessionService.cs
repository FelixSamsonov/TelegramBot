using System.Collections.Concurrent;
using TelegramBot.Models;
using TelegramBot.Services.Abstractions;

namespace TelegramBot.Services.Implementations;

public class InMemoryUserSessionService : IUserSessionService
{
    private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

    public UserSession GetOrCreateSession(long chatId)
    {
        return _sessions.GetOrAdd(chatId, id => new UserSession { ChatId = id });
    }

    public void ResetSession(long chatId)
     => _sessions[chatId] = new UserSession { ChatId = chatId };
}
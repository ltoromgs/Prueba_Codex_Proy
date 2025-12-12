using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISessionTrackerService
{
    Task SetSessionForUser(string username, string sessionId);
    Task<string> GetSessionForUser(string username);
}

public class InMemorySessionTracker : ISessionTrackerService
{
    private static readonly Dictionary<string, string> _activeSessions = new();

    public Task SetSessionForUser(string username, string sessionId)
    {
        _activeSessions[username] = sessionId;
        return Task.CompletedTask;
    }

    public Task<string> GetSessionForUser(string username)
    {
        _activeSessions.TryGetValue(username, out var sessionId);
        return Task.FromResult(sessionId);
    }
}
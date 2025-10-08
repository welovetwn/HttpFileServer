//Services\AuthSessionTracker.cs
using System.Collections.Concurrent;

namespace HttpFileServer.Services
{
    public class AuthSessionTracker
    {
        // 儲存 username -> sessionId
        private readonly ConcurrentDictionary<string, string> _userSessions = new();

        public bool IsAlreadyLoggedIn(string username, string currentSessionId)
        {
            if (_userSessions.TryGetValue(username, out var existingSession))
            {
                return existingSession != currentSessionId;
            }
            return false;
        }

        public void SetSession(string username, string sessionId)
        {
            _userSessions[username] = sessionId;
        }

        public void RemoveSession(string username)
        {
            _userSessions.TryRemove(username, out _);
        }
    }
}

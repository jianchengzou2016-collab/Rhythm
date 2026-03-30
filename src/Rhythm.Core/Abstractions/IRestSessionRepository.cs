using Rhythm.Core.Models;

namespace Rhythm.Core.Abstractions;

public interface IRestSessionRepository
{
    void SaveSession(RestSessionRecord session);

    IReadOnlyList<RestSessionRecord> GetRecentSessions(int limit);

    IReadOnlyList<RestSessionRecord> GetSessionsSince(DateTimeOffset since);
}

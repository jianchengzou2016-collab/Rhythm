using Rhythm.Core.Models;

namespace Rhythm.Core.Events;

public sealed class RestSessionRecordedEventArgs : EventArgs
{
    public RestSessionRecordedEventArgs(RestSessionRecord session)
    {
        Session = session;
    }

    public RestSessionRecord Session { get; }
}

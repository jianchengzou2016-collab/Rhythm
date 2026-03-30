using Rhythm.Core.Abstractions;

namespace Rhythm.Core.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}

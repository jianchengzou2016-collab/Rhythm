namespace Rhythm.Core.Abstractions;

public interface IClock
{
    DateTimeOffset Now { get; }
}

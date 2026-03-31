namespace Rhythm.Core.Models;

public sealed record RhythmSettings(int WorkIntervalMinutes, int RestDurationSeconds, string LanguageCode = "zh-CN")
{
    public static RhythmSettings Default { get; } = new(50, 600, "zh-CN");

    public TimeSpan WorkInterval => TimeSpan.FromMinutes(WorkIntervalMinutes);

    public TimeSpan RestDuration => TimeSpan.FromSeconds(RestDurationSeconds);

    public RhythmSettings Normalize()
    {
        return new RhythmSettings(
            Math.Clamp(WorkIntervalMinutes, 1, 240),
            Math.Clamp(RestDurationSeconds, 5, 3600),
            NormalizeLanguage(LanguageCode));
    }

    private static string NormalizeLanguage(string? languageCode)
    {
        return languageCode switch
        {
            "en-AU" => "en-AU",
            _ => "zh-CN"
        };
    }
}

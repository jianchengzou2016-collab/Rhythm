using Rhythm.Core.Models;

namespace Rhythm.Core.Abstractions;

public interface IRhythmSettingsStore
{
    RhythmSettings LoadSettings();

    void SaveSettings(RhythmSettings settings);
}

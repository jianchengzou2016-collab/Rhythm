namespace Rhythm.App.Localization;

public static class AppText
{
    public static string Get(string languageCode, string key)
    {
        var strings = languageCode == "en-AU" ? English : Chinese;
        return strings.TryGetValue(key, out var value) ? value : key;
    }

    public static string Format(string languageCode, string key, params object[] args)
    {
        return string.Format(Get(languageCode, key), args);
    }

    public static IReadOnlyList<LanguageOption> SupportedLanguages { get; } =
    [
        new("zh-CN", "简体中文"),
        new("en-AU", "English")
    ];

    private static readonly Dictionary<string, string> Chinese = new()
    {
        ["AppName"] = "Rhythm",
        ["AppDescription"] = "帮你在工作与休息之间，找到更稳定的电脑使用节奏。",
        ["CurrentState"] = "当前状态",
        ["StateLocked"] = "已锁屏",
        ["StateWorking"] = "工作中",
        ["StateResting"] = "休息中",
        ["StatusWorking"] = "距离下一次休息还有 {0}",
        ["StatusResting"] = "本次休息还剩 {0}",
        ["StatusLocked"] = "解锁后将重新开始本轮工作计时",
        ["LockHint"] = "锁屏后会自动重置当前计时。",
        ["LockHintLocked"] = "锁屏已触发重置，本轮工作时长不会继续累计。",
        ["SettingsTitle"] = "节奏设置",
        ["WorkIntervalLabel"] = "每隔多久休息一次（分钟）",
        ["RestDurationLabel"] = "每次休息时长（秒）",
        ["LanguageLabel"] = "界面语言",
        ["SaveSettings"] = "保存设置",
        ["Minimize"] = "最小化",
        ["TodayTotal"] = "今日提醒",
        ["TodayCompleted"] = "今日完成休息",
        ["TodayAverage"] = "今日平均实际休息",
        ["RecentHistory"] = "最近休息记录",
        ["HistoryDescription"] = "记录每次提醒的结果、计划时长和实际休息时长。",
        ["HistoryStart"] = "开始时间",
        ["HistoryResult"] = "结果",
        ["HistoryPlanned"] = "计划休息",
        ["HistoryActual"] = "实际休息",
        ["ResultCompleted"] = "已完成",
        ["ResultSkipped"] = "已跳过（ESC）",
        ["ResultInterrupted"] = "因锁屏中断",
        ["OverlayTitle"] = "该休息了",
        ["OverlaySubtitle"] = "放松一下眼睛和肩膀",
        ["OverlayHint"] = "按 ESC 跳过这次休息提醒",
        ["TrayOpen"] = "打开 Rhythm",
        ["TraySkip"] = "跳过本次休息",
        ["TrayExit"] = "退出",
        ["InvalidInteger"] = "请输入有效的整数。",
        ["InvalidWorkInterval"] = "工作间隔请填写 1 到 240 分钟。",
        ["InvalidRestDuration"] = "休息时长请填写 5 到 3600 秒。",
        ["SettingsSaved"] = "节奏设置已保存，并从现在开始重新计时。"
    };

    private static readonly Dictionary<string, string> English = new()
    {
        ["AppName"] = "Rhythm",
        ["AppDescription"] = "Find a steadier rhythm between work and rest.",
        ["CurrentState"] = "Current State",
        ["StateLocked"] = "Locked",
        ["StateWorking"] = "Working",
        ["StateResting"] = "Resting",
        ["StatusWorking"] = "Next break in {0}",
        ["StatusResting"] = "Break ends in {0}",
        ["StatusLocked"] = "The work timer will restart after unlock",
        ["LockHint"] = "Locking the computer resets the current timer.",
        ["LockHintLocked"] = "The timer has been reset because the session is locked.",
        ["SettingsTitle"] = "Rhythm Settings",
        ["WorkIntervalLabel"] = "Break every (minutes)",
        ["RestDurationLabel"] = "Break duration (seconds)",
        ["LanguageLabel"] = "Language",
        ["SaveSettings"] = "Save",
        ["Minimize"] = "Minimize",
        ["TodayTotal"] = "Breaks Today",
        ["TodayCompleted"] = "Completed Today",
        ["TodayAverage"] = "Avg Actual Rest",
        ["RecentHistory"] = "Recent Break History",
        ["HistoryDescription"] = "See the outcome, planned duration, and actual rest time for each break.",
        ["HistoryStart"] = "Started At",
        ["HistoryResult"] = "Result",
        ["HistoryPlanned"] = "Planned",
        ["HistoryActual"] = "Actual",
        ["ResultCompleted"] = "Completed",
        ["ResultSkipped"] = "Skipped (ESC)",
        ["ResultInterrupted"] = "Interrupted by lock",
        ["OverlayTitle"] = "Time for a break",
        ["OverlaySubtitle"] = "Relax your eyes and shoulders",
        ["OverlayHint"] = "Press ESC to skip this break",
        ["TrayOpen"] = "Open Rhythm",
        ["TraySkip"] = "Skip current break",
        ["TrayExit"] = "Exit",
        ["InvalidInteger"] = "Please enter valid integers.",
        ["InvalidWorkInterval"] = "Work interval must be between 1 and 240 minutes.",
        ["InvalidRestDuration"] = "Rest duration must be between 5 and 3600 seconds.",
        ["SettingsSaved"] = "Settings saved. The timer has restarted from now."
    };
}

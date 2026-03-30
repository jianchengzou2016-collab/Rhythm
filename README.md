# Rhythm

Rhythm 是一个运行在 Windows 上的 .NET 桌面工具，目标是帮助人类找到更健康的电脑使用节奏。

当前一期聚焦屏幕使用休息提醒：

- 支持自定义休息节奏：工作间隔、休息时长
- 电脑锁屏后自动重置当前计时
- 休息提醒以全屏半透明遮罩展示，支持 `ESC` 跳过
- 记录每次提醒的结果与实际休息时长

## 技术栈

- .NET 8
- WPF
- SQLite

## 目录结构

```text
src/
  Rhythm.App/             WPF 桌面应用与托盘交互
  Rhythm.Core/            节奏状态机、领域模型、抽象接口
  Rhythm.Infrastructure/  SQLite 持久化
```

## 当前能力

- 节奏设置：自定义工作间隔与休息时长
- Windows 锁屏/解锁监听
- 全屏休息遮罩与倒计时
- `ESC` 跳过当前休息
- SQLite 数据记录
- 最近记录与今日统计面板
- 设置窗口与最小化运行

## 本地运行

```powershell
dotnet build Rhythm.sln
dotnet run --project .\src\Rhythm.App\Rhythm.App.csproj
```

## 数据存储

应用默认将 SQLite 数据库存放在：

```text
%LOCALAPPDATA%\Rhythm\rhythm.db
```

## 开源说明

本仓库使用 MIT License。

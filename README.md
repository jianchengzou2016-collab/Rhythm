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
  Rhythm.App/             WPF 桌面应用与 Windows 平台实现
  Rhythm.Core/            节奏状态机、领域模型、抽象接口
  Rhythm.Infrastructure/  SQLite 持久化
  Rhythm.Mobile/          移动端协调层骨架，供未来 Android UI 复用
```

当前结构已经开始为多端做准备：

- `Rhythm.Core` 保留纯业务逻辑与平台无关接口
- `Rhythm.App` 提供 Windows 专属实现，例如托盘、锁屏监听、多显示器遮罩
- 后续如果新增 Android，可以复用 `Rhythm.Core` 的节奏逻辑、设置模型和数据结构，再替换平台层与 UI 层

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

## 图标与安装包

- 应用图标资源在 `src/Rhythm.App/Assets/`
- 重新生成图标：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Generate-RhythmIcon.ps1
```

- 构建 Windows 10+ 自包含安装包（无需预装 .NET 8 runtime）：
- 构建 Windows 10+ `Inno Setup` 轻量安装包：

```powershell
powershell -ExecutionPolicy Bypass -File .\packaging\windows\Build-RhythmInstaller.ps1 -Configuration Release -RuntimeIdentifier win-x64
```

- 安装脚本在 `packaging/windows/Rhythm.iss`
- 当前安装包不内置应用运行时，因此目标机器需要 `x64 的 .NET Desktop Runtime 8.0.x`
- 安装器会先检测运行时；缺失时会优先尝试用 `winget` 安装 `Microsoft.DotNet.DesktopRuntime.8`，也可跳转到微软官方下载页
- 对 framework-dependent 的 `net8.0` 应用，`.NET` 主线会优先使用已安装的最新 `8.0.x` 补丁版本
- 安装向导会展示 Rhythm 的功能简介，并默认勾选“登录后自动启动”，用户可在安装时取消

- 安装包输出目录：

```text
artifacts/installer/
```

## 版本发布

- 版本号由 Git tag 自动决定，当前约定使用 `v` 前缀，例如 `v1.0.1`
- 平时分支上的构建会基于最近的发布 tag 自动推导下一个补丁版本
- 正式发布时，只需要在要发布的提交上打 tag，再运行安装包构建脚本，或直接 push tag 触发 GitHub Actions 的 release 工作流

```powershell
& 'D:\Program Files\Git\cmd\git.exe' tag v1.0.1
& 'D:\Program Files\Git\cmd\git.exe' push origin v1.0.1
```

## 数据存储

应用默认将 SQLite 数据库存放在：

```text
%LOCALAPPDATA%\Rhythm\rhythm.db
```

## 开源说明

本仓库使用 MIT License。

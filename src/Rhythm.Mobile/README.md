# Rhythm.Mobile

`Rhythm.Mobile` 是为未来 Android 版本准备的移动端骨架项目。

当前这个项目刻意保持为纯 .NET 类库，不直接依赖 WPF、Windows API 或 Android workload。它的职责是：

- 复用 `Rhythm.Core` 的节奏引擎和设置模型
- 为移动端 UI 提供一个更轻的协调层
- 为未来接入 .NET MAUI 或原生 Android 提供稳定入口

## 当前内容

- `MobileShellCoordinator`
- `MobileDashboardState`
- `MobileSessionListItem`

## 后续接入 Android 的建议顺序

1. 安装 `maui` / Android workload
2. 新增真正的 Android 或 MAUI UI 项目
3. 让移动端 UI 依赖 `Rhythm.Mobile`
4. 为移动端补充通知、前台服务、屏幕状态监听等平台实现

当前仓库里没有安装 Android workload，所以这里先提交一个可编译、可继续演进的移动端骨架，而不是伪造一个无法运行的 Android 工程。

# 音频系统

## 初始化与分层

`AudioComponent` 是客户端 Root 的组件，由 `EntryEvent3_InitClient` 在 `GlobalComponent` 和 `ResourcesLoaderComponent` 之后创建。它跨关卡保留，初始化不会播放示例音频。原 `CurrentScene` 挂载已移除。

- `Model/Client/AudioTypes.cs` 定义分组、播放结果、业务事件和旧枚举。
- `ModelView/Client/Audio/` 只保存实体数据和 Unity/YooAsset 引用。`AudioVoice` 是 `AudioComponent` 的子实体。
- `HotfixView/Client/Audio/` 实现播放、更新、回收、设置与事件处理。System 使用 `static partial`、`EntitySystemOf`、`FriendOf`，异步方法使用 `ETTask`。
- Hotfix 不引用播放组件或 Unity 类型，通过 `PlayAudioRequested` 通知客户端。

`GlobalComponentSystem` 自动补建缺失的 `Global/Audio`、`Global/Grid` 子节点，音频组件在 Audio 下创建自己拥有的 `AudioRuntime`。不依赖旧的 `Audio.prefab`。

当前实现服务于本项目的 **2D 音频**。分组为 Music、Sound、UI、Ambient；空间音效、跟随目标和 AudioMixer 效果路由不在本版范围内。

## 配置流程

编辑 `Packages/cn.etetet.yiuilubangen/Assets/Editor/Luban/Datas/Audio.xlsx`，保留前四行 Luban 表头。复制已有数据行后修改字段，不需要新增 C# 枚举。

| 字段 | 含义与约束 |
|---|---|
| Id | 非负且唯一的配置 ID；保留旧 ID 0、1 |
| Path | YooAsset 的资源定位地址；当前示例使用完整项目资源路径，资源必须被收集 |
| Key | 唯一调用名称，区分大小写，不允许首尾空格，例如 `ui.click` |
| Group | `Music`、`Sound`、`UI`、`Ambient`，表格提供下拉列表 |
| Volume | 基础音量，0~1 |
| Loop | 是否循环，由配置决定；Music 也可配置为单次 |
| PitchMin / PitchMax | 随机音高范围，0.1~3，Min 不大于 Max；固定音高时两者填相同值 |
| FadeIn / FadeOut | 淡入、淡出秒数，非负；0 表示立即变化 |
| MaxInstances | 同 ID 同时存在的请求上限，至少 1，包含加载、暂停和淡出中的实例 |
| CooldownMs | 两次被接受请求之间的最短间隔，单位毫秒；0 表示无限制 |
| Priority | Unity AudioSource 优先级，0~256，数值越小越优先；不用于突破系统并发上限 |
| Overflow | `Reject` 拒绝新请求；`ReplaceOldest` 立即回收同 ID 最老实例后播放 |

现有两行是保留的示例资源：`bg1` 为循环音乐，`bg2` 为单次音效。它们不会自动播放。

修改完成后执行 Unity 菜单 `ET/Excel/ExcelExporter`，更新资源构建。字段结构变更后重新编译全部四层（F6），重新进入 Play Mode；不要只热重载 HotfixView 或单独替换二进制配置。配置类与二进制布局必须一致。

同一字段结构下重新加载配置后，调用 `audio.ReloadConfigIndex()` 更新名称索引。无效行会记录 Fiber 日志；重复 Key 的名称调用会被拒绝。直接按 ID 调用仍逐条校验字段。

## HotfixView 调用

下面示例放在已有的 `async ETTask` System 方法中。

```csharp
AudioComponent audio = self.Root().GetComponent<AudioComponent>();
EntityRef<AudioComponent> audioRef = audio;
AudioPlayResult result = await audio.PlayAsync("bg1");
audio = audioRef;
if (audio == null)
{
    return;
}

if (result.Status == AudioPlayStatus.Started ||
    result.Status == AudioPlayStatus.AlreadyPlaying)
{
    long handle = result.Handle;
    // 需要后续控制时，将 handle 保存到业务实体的 long 字段。
}
```

也支持 `await audio.PlayAsync(0)`。`await` 等待资源加载和启动播放，不等待声音结束。

返回值包含 `Handle` 与 `Status`：

- `Started`：已启动，Handle 有效。
- `AlreadyPlaying`：请求的是当前同一首音乐，返回已有 Handle，不从头播放。
- `InvalidConfig`：Key/ID 不存在，名称重复或字段非法。
- `Cooldown`：重复请求过快。
- `CapacityReached`：同 ID 或全局并发达到限制。
- `Cancelled`：加载中被停止、被后续音乐请求替代，或组件已销毁。
- `Failed`：资源缺失、加载失败或播放初始化异常，详情记录 Fiber 日志。

后六种结果的 Handle 为 0。自然结束、显式停止、替换和组件销毁都会使旧 Handle 失效。对象池复用 AudioSource，不复用播放句柄。

```csharp
audio.Pause(handle);
audio.Resume(handle);
audio.Stop(handle);       // 使用该音频的 FadeOut。
audio.Stop(handle, 0f);   // 立即停止。
audio.Stop(handle, 0.3f); // 指定淡出秒数。

audio.PauseGroup(AudioGroup.Sound, true);
audio.PauseGroup(AudioGroup.Sound, false);
audio.StopGroup(AudioGroup.Ambient); // 默认立即停止，包含加载中请求。
audio.StopAll();

bool active = audio.IsActive(handle); // 包含加载、暂停、淡出；不是“当前可听见”。
int count = audio.ActiveCount();
audio.ConfigureLimits(maxVoices: 32, maxPooledSources: 16);
```

降低全局上限不会截断已有声音，只会限制新增请求。`MaxVoices` 至少为 2，音乐交叉淡化也计入此上限。同 ID 溢出替换会立即停止最老实例；正常 Stop 则使用淡出。

## Music 的行为

通过 `PlayAsync` 播放 Group 为 Music 的配置即可切歌，无需另一套接口。

1. 同一首已开始的音乐不会重复启动。
2. 新音乐加载成功并启动后，旧音乐按自己的 FadeOut 淡出。
3. 新资源加载失败时，保留当前音乐。
4. 连续切歌时，只有最新加载请求可以启动。过期加载完成后释放句柄，不播放。
5. 交叉淡化最多保留当前音乐和一条新音乐/淡出实例。下一次切歌会立即清除上次尚未结束的淡出实例。

淡入淡出使用 `Time.unscaledDeltaTime`，不受 `Time.timeScale` 影响。暂停实例或分组时冻结其淡化；暂停状态下 Stop 立即回收，避免等待无法推进的淡化。

## 音量、静音与设置持久化

```csharp
audio.SetMasterVolume(0.8f);
audio.SetGroupVolume(AudioGroup.Music, 0.5f);
audio.SetMuted(false);
audio.SetGroupMuted(AudioGroup.Sound, true);

float musicVolume = audio.GetGroupVolume(AudioGroup.Music);
float masterVolume = audio.GetMasterVolume();
bool soundMuted = audio.IsGroupMuted(AudioGroup.Sound);
audio.SaveSettings(); // 设置界面关闭、应用挂起时可主动落盘。
```

最终音量为：配置 Volume × 淡化增益 × MasterVolume × 分组音量。静音独立保存，不覆盖原音量值。数值输入限制到 0~1，NaN/Infinity 会抛出参数异常。

设置保存在 `PuzzleGame.Audio.*` PlayerPrefs 键下，与游戏存档槽位无关。变更立即生效，最后一次修改后 0.5 秒保存，正常销毁组件时也会保存。强制终止或在防抖时间内挂起可能来不及落盘；应用挂起入口应主动调用 `SaveSettings()`。本次提供设置接口，没有修改设置页 Prefab 或生成绑定。

## Hotfix 事件调用

在 StateSync 客户端 Root 上发布：

```csharp
await EventSystem.Instance.PublishAsync(self.Root(), new PlayAudioRequested
{
    Key = "bg2",
});
```

HotfixView 中 `PlayAudioRequested_Play` 处理该事件。事件方式不返回可操作句柄，需要精确控制时由 HotfixView 业务直接调用 `PlayAsync`。

旧的 `PlayAudioOne(AUDIO.bg2)`、`PlayAudioLoop(AUDIO.bg1)` 保留兼容，但组件获取位置已统一为 Root。旧 `PlayAudioOne` 的 `loop` 参数会覆盖配置循环值，新业务应使用配置驱动的 `PlayAsync`。

## 资源与生命周期

每次加载先预留播放实体和并发名额，使用独立 YooAsset `AssetHandle`。同资源底层共享由 YooAsset 处理，不使用 Root ResourcesLoader 永久持有音频。

- 播放成功后资源句柄交给 AudioVoice；自然结束和停止时，先清除 AudioSource.clip，再释放句柄。
- 加载中的停止是逻辑取消：立即移除播放名额，底层加载完成后释放句柄。不会中断同资源的其他加载。
- 每次 await 后通过 EntityRef 恢复并检查实体，组件销毁后不会继续创建播放对象。
- AudioSource 回收到有上限的对象池；组件销毁时清理活跃实例、池和自己的 AudioRuntime。
- 暂停导致的 `isPlaying == false` 不视为自然结束。

## 验证

隔离回归命令（仓库根执行）：

```powershell
dotnet run --project 'Packages/cn.etetet.gamebase/Tests~/Audio/Audio.Tests.csproj'
```

测试直接链接生产音频源码、ETTask 和 Luban 解码器；Unity、Entity 生命周期与 YooAsset 使用边界替身。覆盖乱序切歌、加载中停止/销毁、资源释放、旧句柄、暂停、并发、音量、淡化、配置和持久化，不能替代 Unity 播放验收。

2026-11-02：实际四层源码使用 Unity 引用和 ET 分析器编译通过；隔离回归 38 项检查通过。全解决方案仍因两个历史 Excel 工程缺失报 MSB3202。尚未完成 Unity Play Mode 听音及发布包验证。

Unity 中还应验收：主菜单首次播放、不同音效重叠、快速切歌、关卡往返、暂停恢复、设置重启恢复、缺失资源日志、后台/恢复，以及长时间播放后 AudioRuntime 对象数量稳定。

Unity API 行为参考：[Unity 6000.0 AudioSource](https://docs.unity3d.com/cn/6000.0/ScriptReference/AudioSource.html)。

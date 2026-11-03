# 音频隔离回归

仓库根执行 `dotnet run --project 'Packages/cn.etetet.gamebase/Tests~/Audio/Audio.Tests.csproj'`。

无需 NuGet 测试框架，任意断言失败均以非零退出码结束。直接编译生产 Audio System/Entity、ETTask、生成 AudioConfig 及 Luban 读取器。Fakes.cs 只提供 Unity、Entity 生命周期和 YooAsset 边界替身，支持手动控制异步加载完成顺序。

该测试验证逻辑与资源所有权，不验证 Unity 的声音输出、真实场景或真实 YooAsset 打包。四层 ET 分析器编译检查和 Unity 验收需分别执行。目录包含 `~`，Unity 不导入测试替身。

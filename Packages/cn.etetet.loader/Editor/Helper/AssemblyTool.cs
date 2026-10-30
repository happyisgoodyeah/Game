using System;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Player;
using UnityEngine;

namespace ET
{
    public static class AssemblyTool
    {
        /// <summary>
        /// Unity线程的同步上下文
        /// </summary>
        static SynchronizationContext unitySynchronizationContext { get; set; }

        /// <summary>
        /// 程序集名字数组
        /// </summary>
        public static readonly string[] DllNames = { "ET.Hotfix", "ET.HotfixView", "ET.Model", "ET.ModelView" };

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            unitySynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// 菜单和快捷键编译按钮
        /// </summary>
        [MenuItem("ET/Loader/Compile _F6", false, ETMenuItemPriority.Compile)]
        static void MenuItemOfCompile()
        {
            // 强制刷新一下，防止关闭auto refresh，文件修改时间不准确
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            DoCompile();
        }

        /// <summary>
        /// 菜单和快捷键热重载按钮
        /// </summary>
        [MenuItem("ET/Loader/Reload _F7", false, ETMenuItemPriority.Compile)]
        static void MenuItemOfReload()
        {
            if (Application.isPlaying)
            {
                CodeLoader.Instance?.Reload();
            }
        }

        /// <summary>
        /// 执行编译代码流程
        /// </summary>
        public static void DoCompile()
        {
            // 强制刷新一下，防止关闭auto refresh，编译出老代码
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (!CheckPlayerBuildSupport())
            {
                return;
            }

            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            CodeModeChangeHelper.ChangeToCodeMode(globalConfig.CodeMode.ToString());

            bool isCompileOk = CompileDlls();
            if (!isCompileOk)
            {
                return;
            }

            CopyHotUpdateDlls();
            
            Log.Info($"Compile Finish!");
        }

        /// <summary>
        /// 缺少 Windows IL2CPP 模块时，Unity 的低级脚本编译接口可能退回编辑器引擎引用。
        /// 先检查安装环境，避免由错误引用引起大量第三方源码编译错误。
        /// </summary>
        static bool CheckPlayerBuildSupport()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
            {
                Debug.LogError($"F6 编译已停止：当前 Unity 未安装 {target} 的构建支持。已有热更 DLL 保留。");
                return false;
            }

            if (target != BuildTarget.StandaloneWindows && target != BuildTarget.StandaloneWindows64)
            {
                return true;
            }

            NamedBuildTarget namedTarget = EditorUserBuildSettings.standaloneBuildSubtarget == StandaloneBuildSubtarget.Server
                ? NamedBuildTarget.Server : NamedBuildTarget.Standalone;
            if (PlayerSettings.GetScriptingBackend(namedTarget) != ScriptingImplementation.IL2CPP)
            {
                return true;
            }

            BuildOptions options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            string playbackDirectory = BuildPipeline.GetPlaybackEngineDirectory(target, options);
            string variationsDirectory = Path.Combine(playbackDirectory, "Variations");
            if (Directory.Exists(variationsDirectory) && Directory.EnumerateDirectories(variationsDirectory, "*il2cpp*").Any())
            {
                return true;
            }

            Debug.LogError($"F6 编译已停止：{target} 使用 IL2CPP，但 Unity {Application.unityVersion} 未安装 Windows Build Support (IL2CPP)。\n" +
                "请在 Unity Hub 中为此版本添加该模块，重启 Unity 后重新编译。仅本地 Mono 调试可改用 Mono；HybridCLR 发布仍需 IL2CPP。\n" +
                $"检查目录：{variationsDirectory}\n已有热更 DLL 保留，本次未执行 Player 脚本编译。");
            return false;
        }

        /// <summary>
        /// 编译成dll
        /// </summary>
        static bool CompileDlls()
        {
            // 运行时编译需要先设置为UnitySynchronizationContext, 编译完再还原为CurrentContext
            SynchronizationContext lastSynchronizationContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(unitySynchronizationContext);

            bool isCompileOk = false;

            try
            {
                Directory.CreateDirectory(Define.BuildOutputDir);
                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
                ScriptCompilationSettings scriptCompilationSettings = new()
                {
                    group = group,
                    target = target,
                    subtarget = group == BuildTargetGroup.Standalone ? (int)EditorUserBuildSettings.standaloneBuildSubtarget : 0,
                    extraScriptingDefines = new[] { "IS_COMPILING" },
                    options = EditorUserBuildSettings.development ? ScriptCompilationOptions.DevelopmentBuild : ScriptCompilationOptions.None
                };
                ScriptCompilationResult result = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, Define.BuildOutputDir);
                string[] compiledAssemblies = result.assemblies.Select(Path.GetFileNameWithoutExtension).ToArray();
                isCompileOk = DllNames.All(name => compiledAssemblies.Contains(name) &&
                    File.Exists($"{Define.BuildOutputDir}/{name}.dll") && File.Exists($"{Define.BuildOutputDir}/{name}.pdb"));
                if (!isCompileOk)
                {
                    Debug.LogError("F6 编译失败：未生成完整的四层 DLL/PDB，已有热更 DLL 保留。请先解决 Console 中的编译错误。");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                SynchronizationContext.SetSynchronizationContext(lastSynchronizationContext);
            }

            return isCompileOk;
        }

        /// <summary>
        /// 将dll文件复制到加载目录
        /// </summary>
        static void CopyHotUpdateDlls()
        {
            Directory.CreateDirectory(Define.CodeDir);
            foreach (string dllName in DllNames)
            {
                string sourceDll = $"{Define.BuildOutputDir}/{dllName}.dll";
                string sourcePdb = $"{Define.BuildOutputDir}/{dllName}.pdb";
                File.Copy(sourceDll, $"{Define.CodeDir}/{dllName}.dll.bytes", true);
                File.Copy(sourcePdb, $"{Define.CodeDir}/{dllName}.pdb.bytes", true);
            }

            AssetDatabase.Refresh();
        }
    }
}

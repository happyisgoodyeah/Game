using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using YIUIFramework;
using YIUIFramework.Editor;

namespace YIUILuban.Editor
{
    public class LubanToolRoot
    {
        [HideInInspector]
        public OdinMenuEditorWindow AutoTool { get; set; }

        [HideInInspector]
        public OdinMenuTree Tree { get; set; }

        internal const string m_LubanName = "Luban";

        private const string m_AllETPkgPath = "Packages/";

        private const string m_AllPkgPath = "Assets/Editor/Luban";

        internal readonly Dictionary<string, string> m_AllExcelData = new();

        public void RefreshAllLuban(OdinMenuEditorWindow autoTool, OdinMenuTree tree)
        {
            this.AutoTool = autoTool;
            this.Tree     = tree;
            this.m_AllExcelData.Clear();
            try
            {
                foreach (var packagePath in Directory.GetDirectories(EditorHelper.GetProjPath(m_AllETPkgPath)))
                {
                    var packageRes = $"{packagePath}/{m_AllPkgPath}";
                    if (Directory.Exists(packageRes))
                    {
                        var etPackagesName = UIOperationHelper.GetETPackagesName(packageRes, false);
                        if (string.IsNullOrEmpty(etPackagesName))
                        {
                            Debug.LogError($"错误这里没有找到ET包名 请检查 {packageRes}");
                            continue;
                        }

                        var resPath = $"{m_AllETPkgPath}{YIUIConstHelper.Const.UIETPackagesFormat}{etPackagesName}/{m_AllPkgPath}";

                        this.m_AllExcelData.TryAdd(etPackagesName, resPath);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"获取所有模块错误 请检查 err={e.Message}{e.StackTrace}");
                return;
            }

            if (!YIUILubanTool.LubanEditorData.RootShowAllConfig)
            {
                var allDatas = new TreeMenuItem<LubanAllDatasModule>(this.AutoTool, this.Tree, $"{m_LubanName}/所有配置", EditorIcons.Folder);
                allDatas.UserData = new LubanAllConfigModuleData
                {
                    AllExcelData = m_AllExcelData
                };
            }

            foreach ((var packagesName, var configPath) in this.m_AllExcelData)
            {
                var data       = YIUILubanTool.LubanEditorData.GetLubanEditorPackageSettings(packagesName);
                var showName   = packagesName;
                var modulePath = $"{m_LubanName}/{packagesName}";
                if (data is { Alias: not null } && !string.IsNullOrEmpty(data.Alias))
                {
                    var alias = data.Alias;
                    showName   = $"{alias}[{packagesName}]";
                    modulePath = $"{m_LubanName}/{showName}";
                }

                var pkgMenu = new TreeMenuItem<LubanConfigModule>(this.AutoTool, this.Tree, modulePath, EditorIcons.Folder);
                pkgMenu.UserData = new LubanConfigModuleData
                {
                    ModulePath   = modulePath,
                    PackagesName = packagesName,
                    ConfigPath   = configPath,
                };
            }
        }
    }
}
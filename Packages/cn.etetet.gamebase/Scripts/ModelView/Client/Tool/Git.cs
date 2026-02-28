using System;
using ET;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [EnableClass]
    public class Git
    {
        #region 右键菜单

        [MenuItem("Assets/Git/Commit", false, 0)]
        public static void Commit()
        {
            CommitInternal().NoContext();
        }

        private static async ETTask CommitInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:commit /path:" + GetSelection() + " /closeonend:0");
        }

        [MenuItem("Assets/Git/Push", false, 1)]
        public static void Push()
        {
            PushInternal().NoContext();
        }

        private static async ETTask PushInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:push /path:" + GetSelection() + " /closeonend:0");
        }

        [MenuItem("Assets/Git/Pull", false, 1)]
        public static void Pull()
        {
            PullInternal().NoContext();
        }

        private static async ETTask PullInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:pull /path:" + GetSelection() + " /closeonend:0");
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Git/Revert", false, 2)]
        public static void Revert()
        {
            RevertInternal().NoContext();
        }

        private static async ETTask RevertInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:revert /path:" + GetSelection() + " /closeonend:0");
        }

        [MenuItem("Assets/Git/Log", false, 51)]
        public static void Log()
        {
            LogInternal().NoContext();
        }

        private static async ETTask LogInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:log /path:" + GetSelection() + " /closeonend:0");
        }

        [MenuItem("Assets/Git/Blame", false, 52)]
        public static void Blame()
        {
            BlameInternal().NoContext();
        }

        private static async ETTask BlameInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:blame /path:" + GetSelection() + " /closeonend:0");
        }

        [MenuItem("Assets/Git/Merge", false, 53)]
        public static void Merge()
        {
            MergeInternal().NoContext();
        }

        private static async ETTask MergeInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:merge /path:" + GetSelection() + " /closeonend:0");
        }

        #endregion

        #region 工具栏菜单项

        [MenuItem("Framework/Git/CommitAll _F4", false, 0)]
        public static void CommitAll()
        {
            CommitAllInternal().NoContext();
        }

        private static async ETTask CommitAllInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:commit /path:" + "Assets*Packages*ProjectSettings" + " /closeonend:0");
        }

        [MenuItem("Framework/Git/PushAll _F5", false, 1)]
        public static void PushAll()
        {
            PushAllInternal().NoContext();
        }

        private static async ETTask PushAllInternal()
        {
            await EditorUtilities.Terminal.ProcessCommandAsync("TortoiseGitProc.exe", "/command:push /path:" + Application.dataPath + " /closeonend:0");
            AssetDatabase.Refresh();
        }

        #endregion

        #region 辅助函数

        /// <summary>
        /// 获取选中路径参数
        /// </summary>
        /// <returns>路径参数</returns>
        public static string GetSelection()
        {
            string path = "Assets";
            string[] strs = Selection.assetGUIDs;
            if (strs != null)
            {
                path = "\"";
                for (int i = 0; i < strs.Length; i++)
                {
                    if (i != 0)
                        path += "*";
                    path += AssetDatabase.GUIDToAssetPath(strs[i]);
                    if (AssetDatabase.GUIDToAssetPath(strs[i]) != "Assets")
                        path += "*" + AssetDatabase.GUIDToAssetPath(strs[i]) + ".meta";
                }

                path += "\"";
            }

            return path;
        }

        /// <summary>
        /// 获取完整路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>完整路径</returns>
        public static string GetCompletePath(string path)
        {
            var completePath = "\"";
            completePath += path;
            if (path != "Assets")
                completePath += "*" + path + ".meta";
            completePath += "\"";
            return path;
        }

        #endregion
    }
}
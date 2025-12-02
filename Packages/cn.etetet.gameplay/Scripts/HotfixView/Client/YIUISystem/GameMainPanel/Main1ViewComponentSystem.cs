using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.10.18
    /// Desc
    /// </summary>
    [FriendOf(typeof(Main1ViewComponent))]
    [FriendOf(typeof(GameMainPanelPanelComponent))]
    public static partial class Main1ViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this Main1ViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this Main1ViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this Main1ViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        [YIUIInvoke(Main1ViewComponent.OnEventArchiveEnterInvoke)]
        private static void OnEventArchiveEnterInvoke(this Main1ViewComponent self)
        {
            OpenArchivePanel().NoContext();
        }


        [YIUIInvoke(Main1ViewComponent.OnEventExitGameInvoke)]
        private static void OnEventExitGameInvoke(this Main1ViewComponent self)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }



        [YIUIInvoke(Main1ViewComponent.OnEventSettingsEnterInvoke)]
        private static void OnEventSettingsEnterInvoke(this Main1ViewComponent self)
        {
            self.UIView.Close();
            YIUIMgrComponent.Inst.Root.OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.SelectLevelView).NoContext();
        }


        [YIUIInvoke(Main1ViewComponent.OnEventStartGameInvoke)]
        private static void OnEventStartGameInvoke(this Main1ViewComponent self)
        {
            self.UIView.Close();
            YIUIMgrComponent.Inst.Root.OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.SelectLevelView).NoContext();
        }

        #endregion YIUIEvent结束>
        
        private static async ETTask<bool> OpenArchivePanel()
        {
            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<ArchivePanelComponent>();
            return true;
        }
    }
}
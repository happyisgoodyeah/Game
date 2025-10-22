using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;
using UnityEditor;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.10.18
    /// Desc
    /// </summary>
    [FriendOf(typeof(GameMainPanelPanelComponent))]
    public static partial class GameMainPanelPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this GameMainPanelPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this GameMainPanelPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this GameMainPanelPanelComponent self)
        {
            self.u_ComSettingBtnButton.gameObject.SetActive(true);
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始

        [YIUIInvoke(GameMainPanelPanelComponent.OnEventOpenSettingInvoke)]
        private static void OnEventOpenSettingInvoke(this GameMainPanelPanelComponent self)
        {
            self.setSettingBtnState(false);
            EnterSettionPanel(self);
        }

        #endregion YIUIEvent结束

        #region selfFunction

        private static async ETTask<bool> EnterSettionPanel(this GameMainPanelPanelComponent self)
        {
            await self.UIPanel.OpenViewAsync<Main1ViewComponent>();
            return true;
        }

        public static void setSettingBtnState(this GameMainPanelPanelComponent self, bool isOpen)
        {
            self.u_ComSettingBtnButton.gameObject.SetActive(isOpen);
        }

        #endregion
    }
}
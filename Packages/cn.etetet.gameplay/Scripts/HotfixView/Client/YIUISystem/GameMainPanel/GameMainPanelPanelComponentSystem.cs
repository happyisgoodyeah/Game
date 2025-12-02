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
        private static async ETTask<bool> YIUIOpen(this ET.Client.GameMainPanelPanelComponent self, ET.Client.EGameMainPanelPanelViewEnum viewEnum)
        {
            await self.UIPanel.OpenViewAsync(viewEnum.ToString());
            return true;
        }
        
        [EntitySystem]
        private static void YIUIInitialize(this GameMainPanelPanelComponent self)
        {
            // 初始默认打开Main1View
            self.UIPanel.OpenViewAsync<Main1ViewComponent>().NoContext();
        }

        [EntitySystem]
        private static void Destroy(this GameMainPanelPanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this GameMainPanelPanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        #endregion YIUIEvent结束
    }
}